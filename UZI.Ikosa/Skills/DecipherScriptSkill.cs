using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Decipher Script", "INT", false, 0d)]
    public class DecipherScriptSkill : SkillBase
    {
        public DecipherScriptSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        // TODO: decipher script
    }
}
