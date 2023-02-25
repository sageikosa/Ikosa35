using System;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Gather Information", "CHA", true, 0d)]
    public class GatherInformationSkill : SkillBase
    {
        public GatherInformationSkill(Creature skillUser)
            :
            base(skillUser)
        {
        }

        public override string SkillName
        {
            get { return "Gather Information"; }
        }

        public override string KeyAbilityMnemonic
        {
            get { return MnemonicCode.Cha; }
        }
    }

}
