using System;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Improved Natural Armor", false),
    AbilityRequirement(Abilities.MnemonicCode.Con, 13),
    NaturalArmorRequirementAttribute()
    ]
    public class ImprovedNaturalArmor: FeatBase
    {
        public ImprovedNaturalArmor(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "Natural armor BaseValue increases by 1"; }
        }

        protected override void OnAdd()
        {
            // add to feat list
            base.OnAdd();

            _Creature.Body.NaturalArmor.BaseValue += 1;
        }

        protected override void OnRemove()
        {
            // remove from feat list
            base.OnRemove();

            if (_Creature.Body.NaturalArmor.BaseValue > 0)
            {
                _Creature.Body.NaturalArmor.BaseValue -= 1;
            }
        }
    }
}
