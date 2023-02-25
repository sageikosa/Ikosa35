using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Deceitful")
    ]
    public class DeceitfulFeat : SkillBoostFeatBase
    {
        public DeceitfulFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(DisguiseSkill);
            yield return typeof(ForgerySkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Disguise and +2 Forgery"; }
        }
    }
}
