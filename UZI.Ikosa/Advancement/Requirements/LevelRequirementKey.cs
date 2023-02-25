namespace Uzi.Ikosa
{
    public class LevelRequirementKey : RequirementKey
    {
        public LevelRequirementKey(string name, int level)
            : base(name)
        {
            Level = level;
        }

        public int Level { get; private set; }
    }
}
