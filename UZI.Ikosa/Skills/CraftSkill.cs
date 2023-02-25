using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Craft", MnemonicCode.Int)]
    public class CraftSkill<F> : SkillBase where F : CraftFocus, new()
    {
        public CraftSkill(Creature skilluser)
            : base(skilluser)
        {
            _Focus = new F();
        }

        protected CraftFocus _Focus;

        public override string SkillName
        {
            get { return string.Format("Craft ({0})", _Focus.FocusName); }
        }
    }
}
