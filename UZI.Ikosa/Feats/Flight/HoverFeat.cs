using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Hover", true),
        NaturalFlightRequirement
    ]
    public class HoverFeat : FeatBase, IActionProvider
    {
        public HoverFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public Guid ID => CoreID;

        public override string Benefit
            => @"Allows hovering as a move action";

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: provide hover
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();

        #endregion
    }
}
