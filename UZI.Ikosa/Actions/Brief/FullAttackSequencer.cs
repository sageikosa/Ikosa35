using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Start of a full attack sequence (after a regular attack)</summary>
    [Serializable]
    public class FullAttackSequencer : ActionBase, ISupplyAttackAction
    {
        /// <summary>Start of a full attack sequence (after a regular attack)</summary>
        public FullAttackSequencer(AttackActionBase atk, string orderKey)
            : base(atk, new ActionTime(TimeType.Brief), atk.ProvokesMelee, atk.ProvokesTarget, orderKey)
        {
            _Budget = null;
            _AtkBudget = null;
        }

        #region private data
        private FullAttackBudget _AtkBudget;
        #endregion

        /// <summary>Underlying attack for the action</summary>
        public AttackActionBase Attack 
            => (AttackActionBase)Source;

        /// <summary>Overrides default IsHarmless true setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless 
            => false;

        public override string Key => $@"Attack.Full.Sequencer.{Attack.Key}";
        public override string DisplayName(CoreActor actor)
            => $@"Full Attack as {Attack.DisplayName(actor)}";

        public FullAttackBudget AttackBudget => _AtkBudget;

        public override bool CombatList 
            => true;

        public override bool IsStackBase(CoreActivity activity)
            => true;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => Attack.GetActivityInfo(activity, observer);

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // must have a full attack budget available
            _Budget = budget as LocalActionBudget;
            if (_Budget.BudgetItems.Any(_bi => _bi.Value is FullAttackBudget))
            {
                _AtkBudget = _Budget.BudgetItems.Items.OfType<FullAttackBudget>().FirstOrDefault(); ;

                // must have started an attack to finish one
                if ((_AtkBudget != null) && _AtkBudget.AttackStarted)
                {
                    // check budget effort levels
                    Attack.CanPerformNow(budget);
                    return base.CanPerformNow(budget);
                }
            }

            // default is this cannot be performed unless all other conditions are met...
            return new ActivityResponse(false);
        }
        #endregion

        protected override CoreStep OnPerformActivity(CoreActivity activity)
            => this.Attack.DoAttack(activity);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _mode in Attack.AimingMode(activity))
                yield return _mode;
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => Attack.IsProvocableTarget(activity, potentialTarget);
    }
}
