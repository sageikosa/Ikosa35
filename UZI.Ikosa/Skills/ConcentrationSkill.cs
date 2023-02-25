using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Concentration", "CON")]
    public class ConcentrationSkill : SkillBase 
    {
        public ConcentrationSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
