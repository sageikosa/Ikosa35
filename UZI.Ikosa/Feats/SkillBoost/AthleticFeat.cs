using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Athletic")
    ]
    public class AthleticFeat : SkillBoostFeatBase
    {
        public AthleticFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(SwimSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Climb and +2 Swim"; }
        }
    }
}
				
