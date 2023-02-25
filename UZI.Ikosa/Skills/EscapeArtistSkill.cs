using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Escape Artist", "DEX", true, 1d)]
    public class EscapeArtistSkill : SkillBase 
    {
        public EscapeArtistSkill(Creature skillUser)
            : base(skillUser)
        {
        }
    }
}
