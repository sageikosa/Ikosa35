using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Ride", MnemonicCode.Dex)]
    public class RideSkill : SkillBase
    {
        public RideSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
