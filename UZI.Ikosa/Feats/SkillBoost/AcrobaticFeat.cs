using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Acrobatic")
    ]
    public class AcrobaticFeat : SkillBoostFeatBase
    {
        public AcrobaticFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(JumpSkill);
            yield return typeof(TumbleSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Jump and +2 Tumble"; }
        }
    }
}
