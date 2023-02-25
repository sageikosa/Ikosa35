using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Knowledge", MnemonicCode.Int, false, 0d)]
    public class KnowledgeSkill<F> : SkillBase where F : KnowledgeFocus, new()
    {
        public KnowledgeSkill(Creature skillUser)
            : base(skillUser)
        {
            _Focus = new F();
        }

        protected KnowledgeFocus _Focus;
        public override string SkillName
        {
            get { return string.Format("Knowledge ({0})", _Focus.FocusName); }
        }
    }
}
