using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Persuasive")
    ]
    public class PersuasiveFeat : SkillBoostFeatBase
    {
        public PersuasiveFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(BluffSkill);
            yield return typeof(IntimidateSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Bluff and +2 Intimidate"; }
        }
    }
}
