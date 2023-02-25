using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Pick Lock", MnemonicCode.Dex, false, 0d)]
    public class PickLockSkill : SkillBase
    {
        public PickLockSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
