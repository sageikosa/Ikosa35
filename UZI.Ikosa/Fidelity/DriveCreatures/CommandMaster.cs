using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Added to a creature that is commanding other creatures through a DriveCreature derived effect</summary>
    [Serializable]
    public class CommandMaster : GroupMasterAdjunct, IActionProvider, IMonitorChange<DeltaValue>, IActionSource
    {
        #region construction
        /// <summary>Added to a creature that is commanding other creatures through a DriveCreature derived effect</summary>
        public CommandMaster(IPowerClass powerClass, CommandGroup group)
            : base(powerClass, group)
        {
            _MaxCmdDice = RecalcMax();
        }
        #endregion

        #region data
        private int _MaxCmdDice;
        #endregion

        /// <summary>Source as IPowerClass</summary>
        public IPowerClass PowerClass
            => Source as IPowerClass;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Add(this, this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Remove(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // track class power level changes to recalc max power dice
            if (oldAnchor == null)
            {
                // setting
                PowerClass.ClassPowerLevel.AddChangeMonitor(this);
            }
            else
            {
                // clearing
                PowerClass.ClassPowerLevel.RemoveChangeMonitor(this);
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }
        #endregion

        #region private int RecalcMax()
        private int RecalcMax()
        {
            // NOTE: source is the power class, PowerDef contains the filter
            var _drive = new Interaction(Creature, Source, null,
                new DriveCreatureData(Creature, CommandGroup.OverwhelmCreatures));
            return PowerClass.ClassPowerLevel.QualifiedValue(_drive);
        }
        #endregion

        /// <summary>Maximum power dice that can be commanded</summary>
        public int MaximumPowerDice
            => _MaxCmdDice;

        /// <summary>Creature that acts as a master to other commanded creatures</summary>
        public Creature Creature
            => Anchor as Creature;

        /// <summary>Group being commanded (and the commander)</summary>
        public CommandGroup CommandGroup
            => Group as CommandGroup;

        public override object Clone()
            => new CommandMaster(PowerClass, CommandGroup);

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (PowerClass.IsPowerClassActive)
            {
                yield return new ReleaseCommand(this, @"100");

                // get budget info
                var _budget = budget as LocalActionBudget;
                if (IsActive && _budget.CanPerformRegular)
                {
                    yield return new SendCommand(this, @"101");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return new AdjunctInfo(@"Commanded Creature", ID);
        }

        #endregion

        #region IMonitorChange<DeltaValue> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { _MaxCmdDice = RecalcMax(); }
        #endregion

        // IActionSource 
        public IVolatileValue ActionClassLevel
            => PowerClass.ClassPowerLevel;
    }
}