using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class AbilityRequirementAttribute : RequirementAttribute
    {
        public AbilityRequirementAttribute(string abilityMnemonic, int minimumValue)
        {
            Mnemonic = abilityMnemonic;
            MinimumValue = minimumValue;
        }

        public readonly string Mnemonic;
        public readonly int MinimumValue;

        public override string Name
            => $@"{Mnemonic} >= {MinimumValue}";

        public override string Description
            => $@"Must have a minimum {Mnemonic} ability score of {MinimumValue} at the designated power die.";

        public override bool MeetsRequirement(Creature creature, int powerLevel)
            => (creature.Abilities[Mnemonic].ValueAtPowerLevel(powerLevel, null) >= MinimumValue);

        public override bool MeetsRequirement(Creature creature)
            => (creature.Abilities[Mnemonic].EffectiveValue >= MinimumValue);

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
            => new AbilityRequirementMonitor(this, target, owner);
    }
}
