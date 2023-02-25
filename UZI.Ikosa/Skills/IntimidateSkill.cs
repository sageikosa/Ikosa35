using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Intimidate", "CHA", true, 0d)]
    public class IntimidateSkill : SkillBase
    {
        public IntimidateSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        // TODO: intimidate to cause to be shaken
    }

}
