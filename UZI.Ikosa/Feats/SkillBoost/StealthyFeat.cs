using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Stealthy")
    ]
    public class StealthyFeat : SkillBoostFeatBase
    {
        public StealthyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(StealthSkill);
            yield return typeof(SilentStealthSkill);
            yield break;
        }

        public override string Benefit
            => @"+2 Hide and +2 Silent Stealth";
    }
}
