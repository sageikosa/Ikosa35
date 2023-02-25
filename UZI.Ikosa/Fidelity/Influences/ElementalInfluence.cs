using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public abstract class ElementalInfluence<Repulse, Overwhelm> : Influence, IActionProvider
        where Repulse : CreatureSubType
        where Overwhelm : CreatureSubType
    {
        #region protected construction
        protected ElementalInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _Battery = new FullResetBattery(this, 3, Day.UnitFactor, 0);
            _Repulse = new SubTypeFilter<Repulse>();
            _Overwhelm = new SubTypeFilter<Overwhelm>();
        }
        #endregion

        #region private data
        private ICreatureFilter _Repulse;
        private ICreatureFilter _Overwhelm;
        private FullResetBattery _Battery;
        #endregion

        public ICreatureFilter RepulseFilter => _Repulse;
        public ICreatureFilter OverwhelmFilter => _Overwhelm;
        public FullResetBattery DriveBattery => _Battery;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Creature;
            if (_critter != null)
            {
                // add battery deltas
                _critter.AddAdjunct(DriveBattery);
                _Battery.MaximumCharges.Deltas.Add(_critter.Abilities.Charisma);
                _Battery.MaximumCharges.Deltas.Add(_critter.ExtraDrivingBattery);

                // add action provider
                _critter.Actions.Providers.Add(this, this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            var _critter = Creature;
            if (_critter != null)
            {
                // remove battery deltas
                _Battery.Eject();
                _Battery.MaximumCharges.Deltas.Remove(_critter.Abilities.Charisma);
                _Battery.MaximumCharges.Deltas.Remove(_critter.ExtraDrivingBattery);

                // remove action provider
                _critter.Actions.Providers.Remove(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override string Description
            => $@"Repulse {RepulseFilter.Description} and overwhelm {OverwhelmFilter.Description} ({DriveBattery.MaximumCharges.QualifiedValue(null)})";

        /// <summary>Provides driving action for a power def</summary>
        protected ActionBase GetDrivingAction(DriveCreaturePowerDef powerDef, ActionTime actionTime, string orderKey)
            => new DriveCreature(new SuperNaturalPowerActionSource(InfluenceClass, 1, powerDef), actionTime, orderKey);

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (InfluenceClass.IsPowerClassActive)
            {
                LocalActionBudget _budget = budget as LocalActionBudget;
                if (_budget != null)
                {
                    // all channellings are regular actions
                    if (_budget.CanPerformRegular)
                    {
                        // must have charges left
                        if (DriveBattery.AvailableCharges > 0)
                        {
                            yield return GetDrivingAction(new RepulseCreatures(1, DriveBattery, RepulseFilter),
                                new ActionTime(TimeType.Regular), @"101");
                            yield return GetDrivingAction(new ReinforceCreatures(1, DriveBattery, OverwhelmFilter),
                                new ActionTime(TimeType.Regular), @"202");
                            yield return GetDrivingAction(new OverwhelmCreatures(1, DriveBattery, OverwhelmFilter),
                                new ActionTime(TimeType.Regular), @"201");
                            yield return GetDrivingAction(new DispelRepelCreatures(1, DriveBattery, OverwhelmFilter),
                                new ActionTime(TimeType.Regular), @"203");
                        }
                    }
                }
            }

            // DONE
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => InfluenceClass.ToPowerClassInfo();

        #endregion
    }

    [Serializable]
    public class SubTypeFilter<SType> : ICreatureFilter
        where SType : CreatureSubType
    {
        #region ICreatureFilter Members

        public bool DoesMatch(Creature critter)
            => critter.SubTypes.Any(_st => _st is SType);

        public string Key => typeof(SType).Name;
        public string Description => typeof(SType).Name;

        #endregion
    }
}