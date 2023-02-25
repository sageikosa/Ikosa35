using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Agile")
    ]
    public class AgileFeat : SkillBoostFeatBase
    {
        public AgileFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(BalanceSkill);
            yield return typeof(EscapeArtistSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Balance and +2 Escape Artist"; }
        }
    }
}
