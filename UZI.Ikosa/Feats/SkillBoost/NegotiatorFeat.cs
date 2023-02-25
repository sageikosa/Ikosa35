using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Negotiator")
    ]
    public class NegotiatorFeat : SkillBoostFeatBase
    {
        public NegotiatorFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(DiplomacySkill);
            yield return typeof(SenseMotiveSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Diplomacy and +2 Sense Motive"; }
        }
    }
}
