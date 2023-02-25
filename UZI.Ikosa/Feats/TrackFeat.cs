using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    [FeatInfo(@"Track", true)]
    public class TrackFeat : FeatBase, IActionProvider
    {
        public TrackFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public Guid ID => CoreID;
        public override string Benefit
            => @"Use Survival skill to find or follow tracks";

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: track actions
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
    }
}
