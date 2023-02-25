using System;
using System.Linq;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class NaturalWeaponRequirementAttribute : RequirementAttribute
    {
        public NaturalWeaponRequirementAttribute(int numberWeapons)
        {
            NumberWeaponsNeeded = numberWeapons;
        }

        public int NumberWeaponsNeeded { get; private set; }

        public override string Name
            => (NumberWeaponsNeeded == 1)
            ? @"Natural Attack"
            : $@">={NumberWeaponsNeeded} Natural Attacks";

        public override string Description
            => (NumberWeaponsNeeded == 1)
            ? @"Natural Attack"
            : $@"Must have {NumberWeaponsNeeded} or more Natural Attacks";

        public override bool MeetsRequirement(Creature creature, int powerLevel)
            => MeetsRequirement(creature);

        public override bool MeetsRequirement(Creature creature)
            => creature.Traits.Count(_t => _t.Trait is NaturalWeaponTrait) >= NumberWeaponsNeeded;

        public override RequirementMonitor CreateMonitor(Uzi.Core.IRefreshable target, Creature owner)
        {
            // TODO: multi-attack requirement monitor?
            return null;
        }
    }
}
