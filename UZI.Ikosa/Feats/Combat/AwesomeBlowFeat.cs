using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Wingover", true),
        AbilityRequirement(MnemonicCode.Str, 25),
        FeatChainRequirement(typeof(PowerAttackFeat)),
        FeatChainRequirement(typeof(ImprovedBullRushFeat)),
        MinimumSizeRequirement(1)
    ]
    public class AwesomeBlowFeat : FeatBase, IActionProvider
    {
        public AwesomeBlowFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit => @"Regular action to attack with damaging blow that can push opponents.";

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.Actions.Providers.Add(this, this);
        }

        protected override void OnDeactivate()
        {
            Creature.Actions.Providers.Remove(this);
            base.OnDeactivate();
        }

        public Guid ID => CoreID;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: awesome blow attack and direction
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
    }
}
