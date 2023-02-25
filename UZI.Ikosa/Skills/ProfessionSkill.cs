using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Profession", MnemonicCode.Wis, false, 0d)]
    public class ProfessionSkill<F> : SkillBase where F : ProfessionFocus, new()
    {
        public ProfessionSkill(Creature skillUser)
            : base(skillUser)
        {
            _Focus = new F();
        }

        protected ProfessionFocus _Focus;

        public override string SkillName
        {
            get { return string.Format("Profession ({0})", _Focus.FocusName); }
        }

        public override bool UseUntrained
        {
            get
            {
                return false;
            }
        }

        public override string KeyAbilityMnemonic
        {
            get { return MnemonicCode.Wis; }
        }
    }
}
