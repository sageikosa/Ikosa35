using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Animal Affinity")
    ]
    public class AnimalAffinityFeat : SkillBoostFeatBase
    {
        public AnimalAffinityFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(HandleAnimalSkill);
            yield return typeof(RideSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Handle Animal and +2 Ride"; }
        }
    }
}
