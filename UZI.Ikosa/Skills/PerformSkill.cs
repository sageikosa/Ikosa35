using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Perform", MnemonicCode.Cha)]
    public class PerformSkill<F> : SkillBase where F : PerformFocus, new()
    {
        public PerformSkill(Creature skillUser)
            : base(skillUser)
        {
            _Focus = new F();
        }

        protected PerformFocus _Focus;
        public override string SkillName
        {
            get { return string.Format("Perform ({0})", _Focus.FocusName); }
        }
    }
}
