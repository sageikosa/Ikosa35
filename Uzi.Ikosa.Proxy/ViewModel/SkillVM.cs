using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class SkillVM : ViewModelBase
    {
        public SkillVM(ActorModel actor, SkillInfo skill)
        {
            _Skill = skill;
            _Take10 = new Take10VM(actor, skill, skill.Take10);
        }

        #region data
        private SkillInfo _Skill;
        private Take10VM _Take10;
        #endregion

        public SkillInfo Skill => _Skill;
        public Take10VM Take10 => _Take10;

        public Visibility ClassSkillVisibility
            => (Skill?.IsClassSkill ?? false)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility NonClassSkillVisibility
            => (Skill?.IsClassSkill ?? false)
            ? Visibility.Collapsed
            : Visibility.Visible;

        /// <summary>
        /// Synchronizes skill info and take 10
        /// </summary>
        /// <param name="skill"></param>
        public void Conformulate(SkillInfo skill)
        {
            // ability update
            _Skill = skill;
            DoPropertyChanged(nameof(Skill));

            // take 10 conform
            _Take10.Conformulate(skill.Take10);
        }
    }
}
