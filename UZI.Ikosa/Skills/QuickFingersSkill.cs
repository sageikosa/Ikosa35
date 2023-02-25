using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Quick Fingers", @"DEX", false, 1d)]
    public class QuickFingersSkill : SkillBase
    {
        public QuickFingersSkill(Creature skillUser)
            :
            base(skillUser)
        {
        }
    }

}
