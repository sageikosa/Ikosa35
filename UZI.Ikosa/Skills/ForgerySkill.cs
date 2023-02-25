using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Forgery", "INT")]
    public class ForgerySkill : SkillBase
    {
        public ForgerySkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
