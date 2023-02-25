using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Advancement
{
    public interface IClassSkills
    {
        bool IsClassSkill(AdvancementClass advClass, SkillBase skill);
    }
}
