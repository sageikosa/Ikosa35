using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class BaseAttackRequirementMonitor : FlexRequirementMonitor<BaseAttackRequirementAttribute, DeltaValue>
    {
        internal BaseAttackRequirementMonitor(BaseAttackRequirementAttribute attr, IRefreshable target, Creature owner)
            : base(attr, target, owner)
        {
            Owner.BaseAttack.AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.BaseAttack.RemoveChangeMonitor(this);
        }
    }
}
