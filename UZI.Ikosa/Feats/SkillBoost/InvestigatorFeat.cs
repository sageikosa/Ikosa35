using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Investigator")
    ]
    public class InvestigatorFeat : SkillBoostFeatBase
    {
        public InvestigatorFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(GatherInformationSkill);
            yield return typeof(SearchSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Gather Information and +2 Search"; }
        }
    }
}
