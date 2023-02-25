using System;

namespace Uzi.Ikosa.Skills
{
    /// <summary>
    /// Information about the skill type (useUntrained defaults to true, checkFactor to 0d)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class SkillInfoAttribute: Attribute 
    {
        public SkillInfoAttribute(string name, string mnemonic)
        {
            Name = name;
            Mnemonic = mnemonic;
            UseUntrained = true;
            CheckFactor = 0d;
        }
        public SkillInfoAttribute(string name, string mnemonic, bool useUntrained, double checkFactor)
        {
            Name = name;
            Mnemonic = mnemonic;
            UseUntrained = useUntrained;
            CheckFactor = checkFactor;
        }
        public string Name { get; private set; }
        public string Mnemonic { get; private set; }
        public bool UseUntrained { get; private set; }
        public double CheckFactor { get; private set; }
    }
}
