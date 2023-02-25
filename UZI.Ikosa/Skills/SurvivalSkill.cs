using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Survival", MnemonicCode.Wis)]
    public class SurvivalSkill : SkillBase
    {
        public SurvivalSkill(Creature skillUser)
            :
            base(skillUser)
        {
        }
    }
}
