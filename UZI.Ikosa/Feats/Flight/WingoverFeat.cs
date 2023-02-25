using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Wingover", true),
        NaturalFlightRequirement
    ]
    public class WingoverFeat : FeatBase, IActionProvider
    {
        public WingoverFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit => @"Reverse flight heading once per round for 10' of flight budget.  Prevents flight climb that round.";

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
            // TODO: action to set any relative flight heading
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
    }
}
