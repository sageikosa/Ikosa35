using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Disable Mechanism", @"INT", false, 0d)]
    public class DisableMechanismSkill : SkillBase 
    {
        public DisableMechanismSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        // TODO: jam a lock
        // TODO: disarm a trap
        // TODO: reset a trap
        // TODO: disarm magic trap (trapfinding?)

    }
}
