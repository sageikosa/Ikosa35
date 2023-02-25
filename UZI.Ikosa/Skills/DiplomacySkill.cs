using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Diplomacy", "CHA")]
    public class DiplomacySkill : SkillBase 
    {
        public DiplomacySkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
