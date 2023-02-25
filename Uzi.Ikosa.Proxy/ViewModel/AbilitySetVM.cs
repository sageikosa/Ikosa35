using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AbilitySetVM : ViewModelBase
    {
        public AbilitySetVM(ActorModel actor, CreatureInfo creature)
        {
            Strength = new AbilityVM(actor, creature.Abilities.Strength);
            Dexterity = new AbilityVM(actor, creature.Abilities.Dexterity);
            Constitution = new AbilityVM(actor, creature.Abilities.Constitution);
            Intelligence = new AbilityVM(actor, creature.Abilities.Intelligence);
            Wisdom = new AbilityVM(actor, creature.Abilities.Wisdom);
            Charisma = new AbilityVM(actor, creature.Abilities.Charisma);
        }

        /// <summary>
        /// Synchronizes all abilities
        /// </summary>
        /// <param name="creature"></param>
        public void Conformulate(CreatureInfo creature)
        {
            Strength.Conformulate(creature.Abilities.Strength);
            Dexterity.Conformulate(creature.Abilities.Dexterity);
            Constitution.Conformulate(creature.Abilities.Constitution);
            Intelligence.Conformulate(creature.Abilities.Intelligence);
            Wisdom.Conformulate(creature.Abilities.Wisdom);
            Charisma.Conformulate(creature.Abilities.Charisma);
        }

        public AbilityVM Strength { get; private set; }
        public AbilityVM Dexterity { get; private set; }
        public AbilityVM Constitution { get; private set; }
        public AbilityVM Intelligence { get; private set; }
        public AbilityVM Wisdom { get; private set; }
        public AbilityVM Charisma { get; private set; }
    }
}
