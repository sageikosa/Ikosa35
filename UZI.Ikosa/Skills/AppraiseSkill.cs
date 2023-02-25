using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Appraise", @"INT")]
    public class AppraiseSkill : SkillBase
    {
        public AppraiseSkill(Creature skilluser)
            : base(skilluser)
        {
        }

        // TODO: appraise item
    }
}
