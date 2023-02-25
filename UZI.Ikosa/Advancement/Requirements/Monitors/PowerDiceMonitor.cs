using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class PowerDiceMonitor<Attr>:FlexRequirementMonitor<Attr, PowerDiceCount>
        where Attr : RequirementAttribute
    {
        public PowerDiceMonitor(Attr attribute, IRefreshable target, Creature owner)
            : base(attribute, target, owner)
        {
            Owner.AdvancementLog.AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.AdvancementLog.RemoveChangeMonitor(this);
        }
    }
}
