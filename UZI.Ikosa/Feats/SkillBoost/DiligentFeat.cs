using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Diligent")
    ]
    public class DiligentFeat : SkillBoostFeatBase
    {
        public DiligentFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(AppraiseSkill);
            yield return typeof(DecipherScriptSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Appraise and +2 Decipher Script"; }
        }
    }
}
