using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    AbilityRequirement(Abilities.MnemonicCode.Int, 13),
    FighterBonusFeat,
    FeatInfo(@"Improved Defensive Combat")
    ]
    public class ImprovedDefensiveCombatFeat : FeatBase, IActionProvider, IActionFilter
    {
        public ImprovedDefensiveCombatFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public Guid ID => CoreID;

        public override string Benefit
            => @"Trade base attack (up to 5) for dodge bonus until next action.";

        protected override void OnActivate()
        {
            Creature.Actions.Providers.Add(this, this);
            Creature.Actions.Filters.Add(this, this);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Creature.Actions.Filters.Remove(this);
            Creature.Actions.Providers.Remove(this);
            base.OnDeactivate();
        }

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget) 
                && _budget.CanPerformRegular)
            {
                yield return new ImprovedDefensiveCombatChoice(Creature);
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();

        #endregion

        #region IActionFilter Members

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // suppress standard defensive combat choice
            return (action is DefensiveCombatChoice);
        }

        #endregion
    }
}
