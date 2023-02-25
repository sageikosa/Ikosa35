using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Nimble Fingers")
    ]
    public class NimbleFingersFeat : SkillBoostFeatBase
    {
        public NimbleFingersFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(DisableMechanismSkill);
            yield return typeof(PickLockSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Disable Device and +2 Open Lock"; }
        }
    }
}
