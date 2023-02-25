using System;

namespace Uzi.Ikosa.Skills
{
    /// <summary>WIS; untrained</summary>
    [Serializable, SkillInfo(@"Spot", @"WIS")]
    public class SpotSkill : SkillBase
    {
        public SpotSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        /// <summary>Max distance to use multiple lines from source when observing or spotting</summary>
        public double MaxMultiDistance => 100 + (3 * EffectiveValue);
    }
}
