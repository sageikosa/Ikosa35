using Uzi.Core.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Packaging;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Actions;
using Uzi.Visualize;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class ItemBase : CoreItem, IItemBase, IMonitorChange<DeltaValue>, ICorePart, IActionSource, IVisible
    {
        #region ctor
        /// <summary>Default: tiny wood for medium creature</summary>
        public ItemBase(string name, Size naturalSize)
            : base(name)
        {
            _Price = new Price(this, false, 0);
            AddIInteractHandler(_Price);
            _ItemMaterial = WoodMaterial.Static;
            _ItemSizer = new ItemSizer(naturalSize, this);
            _Hard = new Deltable(ItemMaterial.Hardness);
            _StructPoints = 1;
            _MaxStructPts = new Deltable(1);
            _MaxStructPts.AddChangeMonitor(this);
        }

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new ItemAttackHandler());
            AddIInteractHandler(new TransitAttackHandler());
            AddIInteractHandler(new SpellTransitHandler());
            base.InitInteractionHandlers();
        }
        #endregion

        #region Data
        private Price _Price;
        protected int _StructPoints;
        protected Deltable _MaxStructPts;
        protected ItemSizer _ItemSizer;
        private Material _ItemMaterial;
        private double _BaseWeight = 0;
        private Deltable _Hard;
        #endregion

        public Price Price => _Price;

        #region public double BaseWeight { get; set; }
        /// <summary>Base weight for an item when at its base size</summary>
        public double BaseWeight
        {
            get { return _BaseWeight; }
            set
            {
                if (_BaseWeight >= 0)
                {
                    _BaseWeight = value;
                    DoPropertyChanged(@"BaseWeight");
                    Weight = 0;
                }
            }
        }
        #endregion

        #region public override double Weight { get; set; }
        public override double Weight
        {
            get { return base.Weight; }
            set
            {
                // recalculate weight...
                CoreSetWeight(BaseWeight * Math.Pow(2, Sizer.Size.Order - ItemSizer.BaseSize.Order));
            }
        }
        #endregion

        #region public int StructurePoints {get; set;}
        public int StructurePoints
        {
            get => _StructPoints;
            set
            {
                if (_StructPoints <= MaxStructurePoints.EffectiveValue)
                    _StructPoints = value;
                else
                    _StructPoints = MaxStructurePoints.EffectiveValue;

                DoPropertyChanged(nameof(StructurePoints));

                if (_StructPoints <= 0)
                {
                    // TODO: leave debris behind?
                    this.UnPath();
                    this.UnGroup();
                    Possessor = null;
                }
            }
        }
        #endregion

        public Deltable MaxStructurePoints => _MaxStructPts;
        public ItemSizer ItemSizer => _ItemSizer;
        public Sizer Sizer => _ItemSizer;
        public IGeometricSize GeometricSize => Sizer.Size.CubeSize();
        public Deltable Hardness => _Hard;
        public int GetHardness() => Hardness.EffectiveValue;
        public Guid PresenterID => ID;

        public bool IsLocal(Locator locator)
            => (this.GetLocated().Locator == locator) || this.HasActiveAdjunct<LocalUnstored>();

        /// <summary>Returns the ItemSize through the ISize interface</summary>
        public Size Size
            => Sizer.Size;

        public virtual int ArmorRating
            => this.GetArmorRating(Sizer);

        public Creature CreaturePossessor
            => Possessor as Creature;

        /// <summary>Basics: every hardness of 5 gives a fall reduce of 1 (10 feet)</summary>
        public virtual double FallReduce
            => Hardness.EffectiveValue / 5d;

        public virtual int MaxFallSpeed => 500;

        #region public Material ItemMaterial { get; set; }
        public Material ItemMaterial
        {
            get => _ItemMaterial;
            set
            {
                if (value != null)
                {
                    // get old structure
                    if (_ItemMaterial != null)
                    {
                        // determine thickness and structure for new material
                        var _oldThick = MaxStructurePoints.BaseDoubleValue / _ItemMaterial.StructurePerInch;

                        // NOTE: moved the following lines into NULL check block (2009-09-04: happy birthday Kira!)
                        // change material, hardness base and structure base
                        _ItemMaterial = value;
                        Hardness.BaseValue = value.Hardness;
                        MaxStructurePoints.BaseDoubleValue = Math.Max(1d, _oldThick * value.StructurePerInch);
                        DoPropertyChanged(@"ItemMaterial");
                    }
                }
            }
        }
        #endregion

        public virtual void AttendeeAdjustments(IAttackSource source, AttackData attack)
            => this.DoAttendeeAdjustments(source, attack);

        #region public static ItemInfoAttribute GetInfo(Type type)
        public static ItemInfoAttribute GetInfo(Type type)
        {
            try
            {
                return (ItemInfoAttribute)type.GetCustomAttributes(typeof(ItemInfoAttribute), true)[0];
            }
            catch
            {
                return new ItemInfoAttribute(type.Name, type.FullName, string.Empty);
            }
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        public virtual void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public virtual void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public virtual void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (sender == _MaxStructPts)
            {
                // when max structure changes, so does item's structure
                var _diff = args.NewValue.Value - args.OldValue.Value;
                StructurePoints += _diff;
            }
        }
        #endregion

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships { get { yield break; } }
        public string TypeName
            => GetType().FullName;

        #endregion

        public override CoreSetting Setting
            => this.GetTokened()?.Token.Context.ContextSet.Setting ?? CreaturePossessor?.Setting;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ObjectInfoFactory.CreateInfo<ObjectInfo>(actor, this, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public virtual bool IsVisible
            => Invisibility.IsVisible(this);

        public virtual IVolatileValue ActionClassLevel
            => new Deltable(1);

        protected override string ClassIconKey
            => string.Empty;

        #region public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        {
            if (AlwaysFailsSave)
                return null;

            // potential save values
            var _deltables = new List<ConstDeltable>();

            // strongest magic-source aura
            var _casterLevel = (from _msaa in Adjuncts.OfType<MagicSourceAuraAdjunct>()
                                where _msaa.IsActive
                                orderby _msaa.CasterLevel descending
                                select new ConstDeltable(Math.Max(_msaa.CasterLevel / 2, 1))).FirstOrDefault();
            if (_casterLevel != null)
                _deltables.Add(_casterLevel);

            // may be multiple attendees?
            ConstDeltable _save(Creature critter)
            {
                switch (saveData.SaveMode.SaveType)
                {
                    case SaveType.Fortitude:
                        return critter.FortitudeSave;

                    case SaveType.Reflex:
                        return critter.ReflexSave;

                    case SaveType.Will:
                        return critter.WillSave;

                    case SaveType.None:
                    case SaveType.NoSave:
                    default:
                        return null;
                }
            }
            foreach (var _attendee in (from _a in Adjuncts.OfType<Attended>()
                                       where _a.IsActive
                                       select _save(_a.Creature)))
                _deltables.Add(_attendee);
            if (_deltables.Any())
                return new BestSoftQualifiedDelta(_deltables.ToArray());
            return null;
        }
        #endregion

        public virtual bool AlwaysFailsSave
            => !this.HasActiveAdjunct<Attended>()
            && !this.HasActiveAdjunct<MagicSourceAuraAdjunct>();
    }
}