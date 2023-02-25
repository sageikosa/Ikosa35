using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Disguise", "CHA")]
    public class DisguiseSkill : SkillBase 
    {
        public DisguiseSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
