using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class AbilityRequirementMonitor : FlexRequirementMonitor<AbilityRequirementAttribute, DeltaValue>
    {
        internal AbilityRequirementMonitor(AbilityRequirementAttribute attr, IRefreshable target, Creature owner)
            : base(attr, target, owner)
        {
            Owner.Abilities[attr.Mnemonic].AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.Abilities[_ReqAttr.Mnemonic].RemoveChangeMonitor(this);
        }
    }
}
