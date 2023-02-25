using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Deft Hands")
    ]
    public class DeftHandsFeat : SkillBoostFeatBase
    {
        public DeftHandsFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(QuickFingersSkill);
            yield return typeof(UseRopeSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Sleight of Hands and +2 Use Rope"; }
        }
    }
}
