using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Ikosa Framework derivation of CoreObject that adds ITacticalAttributes, Armor Rating and Structure Points
    /// </summary>
    [Serializable]
    public abstract class ObjectBase : CoreObject, IVisible, IObjectBase
    {
        #region ctor()
        protected ObjectBase(string name, Material objectMaterial)
            : base(name)
        {
            DoesSupplyCover = CoverLevel.None;
            DoesSupplyConcealment = false;
            DoesSupplyTotalConcealment = false;
            DoesBlocksLineOfEffect = false;
            ObjectMaterial = objectMaterial;
            _SoundDiff = new Deltable(0);
            _Sizer = new ObjectSizer(Size.Medium, this);
            _MaxStrucPts = 5;
            _StrucPts = 5;
        }
        #endregion

        #region data
        protected int _StrucPts;
        protected int _MaxStrucPts;
        protected ObjectSizer _Sizer;
        private CoverLevel _Cover = CoverLevel.None;
        private bool _Conceals = false;
        private bool _AbsConceal = false;
        private bool _BlocksEffect = false;
        private bool _BlocksDetect = false;
        private bool _BlocksTransit = false;
        private bool _HindersTransit = false;
        private bool _BlocksSpread = false;
        private double _Opacity = 1d;
        private Deltable _SoundDiff;
        #endregion

        public bool Masterwork { get; set; }

        #region public virtual int StructurePoints {get; set;}
        public virtual int StructurePoints
        {
            get => _StrucPts;
            set
            {
                if (_StrucPts <= _MaxStrucPts)
                {
                    _StrucPts = value;
                }
                else
                {
                    _StrucPts = _MaxStrucPts;
                }

                DoPropertyChanged(nameof(StructurePoints));

                if (_StrucPts <= 0)
                {
                    // TODO: leave debris behind?
                    this.UnPath();
                    this.UnGroup();
                }
            }
        }
        #endregion

        #region public virtual int MaxStructurePoints {get; set;}
        public virtual int MaxStructurePoints
        {
            get => _MaxStrucPts;
            set
            {
                var _diff = value - _MaxStrucPts;
                if (value > 0)
                {
                    _MaxStrucPts = value;
                    DoPropertyChanged(nameof(MaxStructurePoints));
                    StructurePoints += _diff;
                }
            }
        }
        #endregion

        protected void DoSetMaxStructurePoints(int maxPts)
        {
            MaxStructurePoints = maxPts;
        }

        /// <summary>ItemAttackHandler; TransitAttackHandler; SpellTransitHandler</summary>
        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new ItemAttackHandler());
            AddIInteractHandler(new TransitAttackHandler());
            AddIInteractHandler(new SpellTransitHandler());
            base.InitInteractionHandlers();
        }

        public ObjectSizer ObjectSizer => _Sizer;
        public Sizer Sizer => _Sizer;
        public abstract IGeometricSize GeometricSize { get; }
        public virtual Guid PresenterID => ID;
        // TODO: size deltas should change weight also...

        public virtual Material ObjectMaterial { get; set; }

        public virtual int Hardness
            => ObjectMaterial.Hardness;

        public int GetHardness() => Hardness;

        public virtual int ArmorRating
            => this.GetArmorRating(Sizer);

        public virtual void AttendeeAdjustments(IAttackSource source, AttackData attack)
            => this.DoAttendeeAdjustments(source, attack);

        public virtual bool IsLocatable
            => true;

        /// <summary>Basics: every hardness of 5 gives a fall reduce of 1 (10 feet)</summary>
        public virtual double FallReduce => Hardness / 5d;

        public virtual int MaxFallSpeed => 500;

        // NOTE: these flags indicate whether the object itself can affect these tactical operations

        public CoverLevel DoesSupplyCover { get => _Cover; set => _Cover = value; }
        public bool DoesSupplyConcealment { get => _Conceals; set => _Conceals = value; }
        public bool DoesSupplyTotalConcealment { get => _AbsConceal; set => _AbsConceal = value; }
        public bool DoesBlocksLineOfEffect { get => _BlocksEffect; set => _BlocksEffect = value; }
        public bool DoesBlocksLineOfDetect { get => _BlocksDetect; set => _BlocksDetect = value; }
        public bool BlocksMove { get => _BlocksTransit; set => _BlocksTransit = value; }
        public bool DoesHindersMove { get => _HindersTransit; set => _HindersTransit = value; }
        public bool DoesBlocksSpread { get => _BlocksSpread; set => _BlocksSpread = value; }

        /// <summary>Range of 0..1 (none .. full)</summary>
        public double Opacity { get => _Opacity; set => _Opacity = value; }

        public Deltable ExtraSoundDifficulty => _SoundDiff;

        #region ITacticalInquiry Members
        protected bool ContainsCell(in TacticalInfo tactical)
            => this.GetLocated()?.Locator.GeometricRegion.ContainsCell(tactical.TacticalCellRef) ?? true;

        public virtual CoverLevel SuppliesCover(in TacticalInfo tacticalInfo)
            => (DoesSupplyCover > CoverLevel.None) && ContainsCell(in tacticalInfo)
            ? DoesSupplyCover : CoverLevel.None;

        public virtual bool SuppliesConcealment(in TacticalInfo tacticalInfo)
            => DoesSupplyConcealment && ContainsCell(in tacticalInfo);

        public virtual bool SuppliesTotalConcealment(in TacticalInfo tacticalInfo)
            => DoesSupplyTotalConcealment && ContainsCell(in tacticalInfo);

        public virtual bool BlocksLineOfEffect(in TacticalInfo tacticalInfo)
            => DoesBlocksLineOfEffect && ContainsCell(in tacticalInfo);

        public virtual bool BlocksLineOfDetect(in TacticalInfo tacticalInfo)
            => DoesBlocksLineOfDetect && ContainsCell(in tacticalInfo);

        public virtual bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            // TODO: source or target cell must be in locator
            return DoesBlocksSpread;
        }

        public bool CanBlockSpread => DoesBlocksSpread;

        #endregion

        public virtual bool IsVisible
            => Invisibility.IsVisible(this);

        public override CoreSetting Setting
            => this.GetTokened()?.Token.Context.ContextSet.Setting;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ObjectInfoFactory.CreateInfo<ObjectInfo>(actor, this);

        /// <summary>Used when the ObjectBase is acting as an IActionProvider</summary>
        public virtual Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #region IMoveAffecter Members
        public virtual bool BlocksTransit(MovementTacticalInfo moveTactical)
            => BlocksMove
            && !moveTactical.Movement.CanMoveThrough(ObjectMaterial)
            && this.InLocator(moveTactical);

        public virtual bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            // TODO: ?
            return false;
        }

        public virtual IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // TODO: ?
            yield break;
        }

        public virtual double HedralTransitBlocking(MovementTacticalInfo moveTactical)
            => BlocksMove && !moveTactical.Movement.CanMoveThrough(ObjectMaterial) && this.InLocator(moveTactical)
            ? 1d
            : 0d;

        public virtual bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
            => !BlocksMove || movement.CanMoveThrough(ObjectMaterial) || !this.InLocator(region);

        public virtual bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
            => DoesHindersMove && !movement.CanMoveThrough(ObjectMaterial) && this.InLocator(region);

        #endregion

        #region public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        {
            if (AlwaysFailsSave)
            {
                return null;
            }

            // potential save values
            var _deltables = new List<ConstDeltable>();

            // strongest magic-source aura
            var _casterLevel = (from _msaa in Adjuncts.OfType<MagicSourceAuraAdjunct>()
                                where _msaa.IsActive
                                orderby _msaa.CasterLevel descending
                                select new ConstDeltable(Math.Max(_msaa.CasterLevel / 2, 1))).FirstOrDefault();
            if (_casterLevel != null)
            {
                _deltables.Add(_casterLevel);
            }

            // may be multiple attendees?
            ConstDeltable _save(Creature critter)
                => saveData.SaveMode.SaveType switch
                {
                    SaveType.Fortitude => critter.FortitudeSave,
                    SaveType.Reflex => critter.ReflexSave,
                    SaveType.Will => critter.WillSave,
                    _ => null,
                };

            foreach (var _attendee in (from _a in Adjuncts.OfType<Attended>()
                                       where _a.IsActive
                                       select _save(_a.Creature)))
            {
                _deltables.Add(_attendee);
            }

            if (_deltables.Any())
            {
                return new BestSoftQualifiedDelta(_deltables.ToArray());
            }

            return null;
        }
        #endregion

        public virtual bool AlwaysFailsSave
            => !this.HasActiveAdjunct<Attended>();
    }
}