using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Heal", "WIS", true, 0d)]
    public class HealSkill : SkillBase
    {
        public HealSkill(Creature skillUser)
            : base(skillUser)
        {
        }

    }
}
