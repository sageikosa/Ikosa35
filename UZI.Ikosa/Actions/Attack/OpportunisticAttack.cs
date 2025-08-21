using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase (Opportunistic)]</summary>
    [Serializable]
    public class OpportunisticAttack : ActionBase, ISupplyAttackAction
    {
        #region construction
        /// <summary>[ActionBase (Opportunistic)]</summary>
        public OpportunisticAttack(AttackActionBase source, CoreActivity opportunity, string orderKey)
            : base(source, new ActionTime(TimeType.Opportunistic), source.ProvokesMelee, source.ProvokesTarget, orderKey)
        {
            _OpptBudget = null;
            _Opportunity = opportunity;
        }
        #endregion

        #region data
        private CoreActivity _Opportunity;
        private OpportunityBudget _OpptBudget;
        #endregion

        /// <summary>Underlying attack for the action</summary>
        public AttackActionBase Attack => (AttackActionBase)Source;

        public CoreActivity Opportunity => _Opportunity;

        public override string Key => $@"Attack.Opportunistic.{Attack.Key}";
        public override string DisplayName(CoreActor actor) => $@"Opportunistic Attack as {Attack.DisplayName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => Attack.GetActivityInfo(activity, observer);

        /// <summary>Overrides default IsHarmless true setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless
            => false;

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            _Budget = budget as LocalActionBudget;
            _OpptBudget = _Budget.BudgetItems[typeof(OpportunityBudget)] as OpportunityBudget;
            if (_OpptBudget.Available > 0)
            {
                Attack.CanPerformNow(budget);
                return base.CanPerformNow(budget);
            }
            else
            {
                return new ActivityResponse(false);
            }
        }

        public override void ProcessManagerInitialized(CoreActivity activity)
            => _OpptBudget?.RegisterOpportunity(_Opportunity);  // register that the opportunity has been taken

        protected override CoreStep OnPerformActivity(CoreActivity activity)
            => Attack.DoAttack(activity);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => Attack.AimingMode(activity);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => Attack.IsProvocableTarget(activity, potentialTarget);
    }
}
