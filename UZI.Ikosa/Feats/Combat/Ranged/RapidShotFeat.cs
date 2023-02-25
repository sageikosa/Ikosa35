using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Abilities;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatChainRequirement(typeof(PointBlankShotFeat)),
    AbilityRequirement(MnemonicCode.Dex, 13),
    FighterBonusFeat,
    FeatInfo(@"Rapid Shot", true)
    ]
    public class RapidShotFeat : FeatBase, IActionProvider
    {
        public RapidShotFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public Guid ID => CoreID;
        public override string Benefit
            => @"One extra ranged attack per round using a full attack.  All attacks take a -2 penalty.";

        protected override void OnActivate()
        {
            Creature.Actions.Providers.Add(this, this);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Creature.Actions.Providers.Remove(this);
            base.OnDeactivate();
        }

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget)
                && _budget.CanPerformRegular)
            {
                yield return new RapidShotChoice(Creature);
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();

        #endregion
    }
}