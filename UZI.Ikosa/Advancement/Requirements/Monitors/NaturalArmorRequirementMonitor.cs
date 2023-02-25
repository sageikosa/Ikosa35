using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class NaturalArmorRequirementMonitor : FlexRequirementMonitor<NaturalArmorRequirementAttribute, DeltaValue>
    {
        internal NaturalArmorRequirementMonitor(NaturalArmorRequirementAttribute attr, IRefreshable target, Creature owner)
            : base(attr, target, owner)
        {
            Owner.BodyDock.NaturalArmorModifier.AddChangeMonitor(this);
        }

        public override void DoTerminate()
        {
            Owner.BodyDock.NaturalArmorModifier.RemoveChangeMonitor(this);
        }
    }
}
