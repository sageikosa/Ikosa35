using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Feats
{
    /// <summary>
    /// +2 Listen and +2 Spot
    /// </summary>
    [
    Serializable,
    FeatInfo("Alertness")
    ]
    public class AlertnessFeat : SkillBoostFeatBase
    {
        /// <summary>
        /// +2 Listen and +2 Spot
        /// </summary>
        public AlertnessFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override IEnumerable<Type> SkillTypes()
        {
            yield return typeof(ListenSkill);
            yield return typeof(SpotSkill);
            yield break;
        }

        public override string Benefit
        {
            get { return "+2 Listen and +2 Spot"; }
        }
    }
}
