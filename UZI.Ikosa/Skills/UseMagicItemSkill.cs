using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Use Magic Item", MnemonicCode.Cha, false, 0d)]
    public class UseMagicItemSkill : SkillBase
    {
        public UseMagicItemSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
