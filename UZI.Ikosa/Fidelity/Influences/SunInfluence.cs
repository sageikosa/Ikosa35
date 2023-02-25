using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class SunInfluence : Influence, IActionProvider, ICreatureFilter
    {
        public SunInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _Battery = null;
        }

        #region private data
        private WrapperBattery _Battery;
        #endregion

        /// <summary>Wraps the battery for the drive undead adjunct</summary>
        public WrapperBattery DriveBattery { get { return _Battery; } }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Creature != null)
            {
                Creature.Actions.Providers.Add(this, this);
                var _undead = Creature.Adjuncts.OfType<DriveUndeadAdjunct>().FirstOrDefault();
                if (_undead != null)
                {
                    _Battery = new WrapperBattery(this, 1, _undead.DriveBattery);
                    Creature.AddAdjunct(_Battery);
                }
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Creature != null)
            {
                Creature.Actions.Providers.Remove(this);
                if (_Battery != null)
                {
                    Creature.RemoveAdjunct(_Battery);
                }
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override string Name { get { return @"Sun Influence"; } }

        public override string Description
        {
            get { return @"Once per day, all undead which would be repulsed are instead destroyed."; }
        }

        public override object Clone() { return new SunInfluence(Devotion, InfluenceClass); }

        #region protected ActionBase GetDrivingAction(DriveCreaturePowerDef powerDef, ActionTime actionTime)
        /// <summary>Provides driving action for a power def</summary>
        protected ActionBase GetDrivingAction(DriveCreaturePowerDef powerDef, ActionTime actionTime, string orderKey)
        {
            var _source = new SuperNaturalPowerActionSource(InfluenceClass, 1, powerDef);
            return new DriveCreature(_source, actionTime, orderKey);
        }
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (InfluenceClass.IsPowerClassActive)
            {
                LocalActionBudget _budget = budget as LocalActionBudget;
                if (_budget != null)
                {
                    // all channellings are regular actions
                    if (_budget.CanPerformRegular && DriveBattery.AvailableCharges > 0)
                    {
                        yield return GetDrivingAction(new DestroyCreatures(1, DriveBattery, this),
                            new ActionTime(TimeType.Regular), @"102");
                    }
                }
            }

            // DONE
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return this.InfluenceClass.ToPowerClassInfo();
        }

        #endregion

        #region ICreatureFilter Members

        public bool DoesMatch(Creature critter)
        {
            return critter.CreatureType is UndeadType;
        }

        public string Key { get { return typeof(UndeadType).Name; } }
        string ICreatureFilter.Description { get { return typeof(UndeadType).Name; } }

        #endregion
    }
}