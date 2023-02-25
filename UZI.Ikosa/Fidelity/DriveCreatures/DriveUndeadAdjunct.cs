using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Adjunct that provides drive undead capabilities to a creature</summary>
    [Serializable]
    public class DriveUndeadAdjunct : Adjunct, ICreatureFilter, IActionProvider, IPowerClass, ICreatureBound
    {
        #region protected constructor
        protected DriveUndeadAdjunct(bool positive)
            : base(typeof(DriveUndeadAdjunct))
        {
            _PowerLevel = new DeltableQualifiedDelta(0, @"Drive Undead Power Level", this);
            _DriveBattery = new FullResetBattery(this, 3, Day.UnitFactor, 0);
            _Sources = new Collection<IPowerClass>();
            _IsPositive = positive;
        }
        #endregion

        #region data
        private DeltableQualifiedDelta _PowerLevel;
        private FullResetBattery _DriveBattery;
        private Collection<IPowerClass> _Sources;
        private bool _IsPositive;
        #endregion

        #region public static DriveUndeadAdjunct AddSource(Creature critter, IPowerClass source, bool positive)
        public static void AddSource(Creature critter, IPowerClass source, bool positive)
        {
            // find
            var _drive = critter.Adjuncts.OfType<DriveUndeadAdjunct>().FirstOrDefault(_d => _d.IsPositiveChannel == positive);
            if (_drive == null)
            {
                // not found, so create
                _drive = new DriveUndeadAdjunct(positive);
                critter.AddAdjunct(_drive);
            }

            // add source
            if (!_drive._Sources.Contains(source))
            {
                _drive._Sources.Add(source);
                _drive._PowerLevel.Deltas.Add(source);
            }
        }
        #endregion

        #region public static IEnumerable<DriveUndeadAdjunct> RemoveSource(Creature critter, IPowerClass source)
        public static void RemoveSource(Creature critter, IPowerClass source)
        {
            // find each source this class is aprt of (should only be one...)
            foreach (var _drive in critter.Adjuncts.OfType<DriveUndeadAdjunct>()
                .Where(_d => _d._Sources.Contains(source))
                .ToList())
            {
                _drive._Sources.Remove(source);
                _drive._PowerLevel.Deltas.Remove(source);

                // empty? eject!
                if (_drive._Sources.Count == 0)
                    _drive.Eject();
            }
        }
        #endregion

        /// <summary>Power battery for driving undead powers</summary>
        public FullResetBattery DriveBattery => _DriveBattery;

        /// <summary>True if the undead driving power is positively charged</summary>
        public bool IsPositiveChannel { get => _IsPositive; set => _IsPositive = value; }

        /// <summary>All sources for the power</summary>
        public IEnumerable<IPowerClass> Sources
            => _Sources.AsEnumerable();

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Creature != null)
            {
                Creature.AddAdjunct(DriveBattery);
                DriveBattery.MaximumCharges.Deltas.Add(Creature.Abilities.Charisma);
                DriveBattery.MaximumCharges.Deltas.Add(Creature.ExtraDrivingBattery);
                Creature.Actions.Providers.Add(this, this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Creature != null)
            {
                DriveBattery.Eject();
                DriveBattery.MaximumCharges.Deltas.Remove(Creature.Abilities.Charisma);
                DriveBattery.MaximumCharges.Deltas.Remove(Creature.ExtraDrivingBattery);
                Creature.Actions.Providers.Remove(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override bool IsProtected
            => true;

        #region IPowerClass Members

        public string ClassName => _Sources.FirstOrDefault()?.ClassName ?? @"Undead Turner";

        public string ClassIconKey => _Sources.FirstOrDefault()?.ClassIconKey ?? @"undead_turn_class";

        public IVolatileValue ClassPowerLevel => _PowerLevel;

        public Guid OwnerID
            => Creature?.ID ?? Guid.Empty;

        public bool IsPowerClassActive { get => Sources.Any(_pc => _pc.IsPowerClassActive); set { } }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _PowerLevel.QualifiedDeltas(qualify);

        private TerminateController _TCtrl;
        private TerminateController _Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        // ICreatureBound
        public Creature Creature
            => Anchor as Creature;

        #region ICreatureFilter Members

        public bool DoesMatch(Creature critter)
            => critter.CreatureType is UndeadType;

        public string Key
            => typeof(UndeadType).Name;

        string ICreatureFilter.Description
            => typeof(UndeadType).Name;

        #endregion

        /// <summary>Provides driving action for a power def</summary>
        protected ActionBase GetDrivingAction(DriveCreaturePowerDef powerDef, ActionTime actionTime, string orderKey)
            => new DriveCreature(new SuperNaturalPowerActionSource(this, 1, powerDef), actionTime, orderKey);

        #region public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsPowerClassActive)
            {
                if (budget is LocalActionBudget _budget)
                {
                    // all channellings are regular actions
                    if (_budget.CanPerformRegular)
                    {
                        // must have charges left
                        if (DriveBattery.AvailableCharges > 0)
                        {
                            if (IsPositiveChannel)
                            {
                                yield return GetDrivingAction(new RepulseCreatures(1, DriveBattery, this),
                                    new ActionTime(TimeType.Regular), @"101");
                            }
                            else if (!IsPositiveChannel)
                            {
                                yield return GetDrivingAction(new ReinforceCreatures(1, DriveBattery, this),
                                    new ActionTime(TimeType.Regular), @"102");
                                yield return GetDrivingAction(new OverwhelmCreatures(1, DriveBattery, this),
                                    new ActionTime(TimeType.Regular), @"101");
                                yield return GetDrivingAction(new DispelRepelCreatures(1, DriveBattery, this),
                                    new ActionTime(TimeType.Regular), @"103");
                            }
                        }

                    }
                }
            }

            // base actions
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Drive Undead", ID);
        #endregion

        public override object Clone()
            => new DriveUndeadAdjunct(IsPositiveChannel);

        public PowerClassInfo ToPowerClassInfo()
            => new PowerClassInfo
            {
                OwnerID = OwnerID.ToString(),
                ID = ID,
                Key = Key,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                IsPowerClassActive = IsPowerClassActive,
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };
    }
}