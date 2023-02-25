using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Use Rope", MnemonicCode.Dex)]
    public class UseRopeSkill : SkillBase
    {
        public UseRopeSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
