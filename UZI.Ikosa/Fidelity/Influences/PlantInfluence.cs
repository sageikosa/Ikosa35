using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class PlantInfluence : Influence, IActionProvider, ICreatureFilter
    {
        public PlantInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _Battery = new FullResetBattery(this, 3, Day.UnitFactor, 0);
        }

        private FullResetBattery _Battery;

        public FullResetBattery DriveBattery => _Battery;
        public override string Name => @"Plant Influence";

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Creature;
            if (_critter != null)
            {
                // add battery deltas
                _critter.AddAdjunct(_Battery);
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
            => $@"Overwhelm Plants ({DriveBattery.MaximumCharges.QualifiedValue(null)})";

        public override object Clone()
            => new PlantInfluence(Devotion, InfluenceClass);

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

            // DONE
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => InfluenceClass.ToPowerClassInfo();

        #endregion

        #region ICreatureFilter Members

        public bool DoesMatch(Creature critter)
            => critter.CreatureType is PlantType;

        public string Key => @"PlantType";

        string ICreatureFilter.Description => @"Plants";

        #endregion
    }
}
