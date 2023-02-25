using System;

namespace Uzi.Ikosa.Skills
{
    /// <summary>DEX; untrained; check</summary>
    [Serializable, SkillInfo(@"Stealth", @"DEX", true, 1d)]
    public class StealthSkill : SkillBase
    {
        public StealthSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }

}
