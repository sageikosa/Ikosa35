using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class CasterLevelRequirementAttribute : RequirementAttribute
    {
        public CasterLevelRequirementAttribute(int minimumValue)
        {
            MinimumValue = minimumValue;
        }

        public readonly int MinimumValue;

        public override string Name { get { return string.Format(@"Caster Level >= {0}", MinimumValue); } }
        public override string Description { get { return string.Format(@"Caster Level for at least one casting class must be {0} or higher at the designated power level.", MinimumValue); } }

        public override bool MeetsRequirement(Creature creature, int powerLevel)
        {
            // must have a caster class
            foreach (var _castClass in creature.Classes.OfType<ICasterClass>())
            {
                // NOTE: this specific version of meets requirement is used when attempting to lock level
                // NOTE: so using the effective locked level is appropriate and accurate
                if (_castClass.EffectiveLevel.QualifiedValue(PowerLevelCheck.LevelCheck(powerLevel)) >= MinimumValue)
                {
                    return true;
                }
            }

            // no caster classes having the caster level at the power level
            return false;
        }

        public override bool MeetsRequirement(Creature creature)
        {
            return MeetsRequirement(creature, creature.AdvancementLog.NumberPowerDice);
        }

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return new PowerDiceMonitor<CasterLevelRequirementAttribute>(this, target, owner);
        }
    }
}
