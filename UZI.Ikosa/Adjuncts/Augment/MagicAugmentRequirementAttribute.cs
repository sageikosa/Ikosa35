using System;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable, AttributeUsage(AttributeTargets.Class)]
    public class MagicAugmentRequirementAttribute : Attribute
    {
        public MagicAugmentRequirementAttribute(int casterLevel, Type craftFeat, params Type[] spells)
        {
            CasterLevel = casterLevel;
            CraftFeat = craftFeat;
            Spells = spells;
        }

        public int CasterLevel { get; set; }
        public Type CraftFeat { get; set; }
        public Type[] Spells { get; set; }
    }
}
