namespace Uzi.Ikosa
{
    public class LearnedSpellRequirementKey: LevelRequirementKey
    {
        public LearnedSpellRequirementKey(string name, int level, int spellLevel, int index)
            : base(name, level)
        {
            SpellLevel = spellLevel;
            SpellIndex = index;
        }

        /// <summary>Level of the spell to learn</summary>
        public int SpellLevel { get; private set; }

        /// <summary>Index of the spell</summary>
        public int SpellIndex { get; private set; }
    }
}
