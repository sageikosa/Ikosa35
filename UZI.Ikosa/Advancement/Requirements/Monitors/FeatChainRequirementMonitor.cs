using System;
using Uzi.Core;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class FeatChainRequirementMonitor : FlexRequirementMonitor<FeatChainRequirementAttribute, FeatBase>
    {
        internal FeatChainRequirementMonitor(FeatChainRequirementAttribute attr, IRefreshable target, Creature owner)
            : base(attr, target, owner)
        {
            Owner.Feats.AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.Feats.RemoveChangeMonitor(this);
        }
    }
}
