using System;
using Uzi.Core;

namespace Uzi.Ikosa.Skills
{
    [
    Serializable,
    SkillInfo(@"Learn Language", @"", false, 0d),
    SourceInfo(@"Language Skill")
    ]
    public class LanguageSkill : SkillBase
    {
        public LanguageSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        public override bool IsClassSkill { get { return true; } }
        public override bool IsClassSkillAtPowerLevel(int powerLevel) { return true; }
    }
}