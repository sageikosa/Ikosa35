using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Flyby Attack", true),
        NaturalFlightRequirement
    ]
    public class FlybyAttackFeat : FeatBase, IActionProvider
    {
        public FlybyAttackFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit => @"While flying, perform regular action at any point during brief movement; instead of just at start or end.";

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
            // TODO: flyby attack action selection
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
    }
}
