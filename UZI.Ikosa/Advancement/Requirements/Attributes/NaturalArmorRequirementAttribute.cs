using System;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class NaturalArmorRequirementAttribute : RequirementAttribute
    {
        public override string Name
        {
            get { return "Natural Armor >= 1"; }
        }

        public override string Description
        {
            get { return "Must have effective natural armor."; }
        }

        public override bool MeetsRequirement(Creature creature, int powerLevel)
        {
            return this.MeetsRequirement(creature);
        }

        public override bool MeetsRequirement(Creature creature)
        {
            return creature.Body.NaturalArmor.BaseValue > 0;
        }

        public override RequirementMonitor CreateMonitor(Uzi.Core.IRefreshable target, Creature owner)
        {
            return null;
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
