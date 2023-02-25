using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Represents a touch spell withheld as a charge in hand (either due to not being used, or a missed attack)
    /// </summary>
    [Serializable]
    public class HeldCharge : SlottedItemBase, IActionProvider
    {
        public HeldCharge(string name, string slotType, CoreActivity originalActivity)
            : base(name, slotType)
        {
            _OrigActivity = originalActivity;
        }

        private CoreActivity _OrigActivity;

        public CoreActivity OriginalActivity
            => _OrigActivity;

        public CastSpell CastSpell
            => _OrigActivity.Action as CastSpell;

        public override bool IsTargetable => false;

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_budget.CanPerformRegular)
            {
                yield return new DeliverHeldCharge(this, @"101");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        protected override string ClassIconKey => string.Empty;

        public override bool IsTransferrable
        {
            // TODO: perhaps this should be true?
            get { return false; }
        }

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool UnslottingProvokes => false;
    }
}
