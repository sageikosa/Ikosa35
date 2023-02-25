using System;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class BaseAttackRequirementAttribute : RequirementAttribute
    {
        public BaseAttackRequirementAttribute(int minimumValue)
        {
            MinimumValue = minimumValue;
        }

        public readonly int MinimumValue;

        public override string Name
        {
            get { return string.Format(@"Base Attack >= {0}", MinimumValue); }
        }

        public override string Description
        {
            get { return string.Format(@"Base Attack must be {0} or higher at the designated power level.", MinimumValue); }
        }

        public override bool MeetsRequirement(Creature creature, int powerLevel)
        {
            return (creature.AdvancementLog.BaseAttackAt(powerLevel) >= MinimumValue);
        }

        public override bool MeetsRequirement(Creature creature)
        {
            return (creature.BaseAttack.EffectiveValue >= MinimumValue);
        }

        public override RequirementMonitor CreateMonitor(Uzi.Core.IRefreshable target, Creature owner)
        {
            return new BaseAttackRequirementMonitor(this, target, owner);
        }
    }
}
