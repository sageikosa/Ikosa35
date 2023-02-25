using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Self-Sufficient")
    ]
    public class SelfSufficientFeat : SkillBoostFeatBase
    {
        public SelfSufficientFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(HealSkill);
            yield return typeof(SurvivalSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Heal and +2 Survival"; }
        }
    }
}
