using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.BodyType;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class ItemSlotBodyMonitor: FlexRequirementMonitor<ItemSlotRequirementAttribute, Body>
    {
        internal ItemSlotBodyMonitor(ItemSlotRequirementAttribute attr, IRefreshable target, Creature owner)
            : base(attr, target, owner)
        {
            Owner.BodyDock.AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.BodyDock.RemoveChangeMonitor(this);
        }
    }
}
