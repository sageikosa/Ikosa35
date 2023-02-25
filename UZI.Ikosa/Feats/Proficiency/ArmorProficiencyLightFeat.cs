using System;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Armor Proficiency (Light)")
    ]
    public class ArmorProficiencyLightFeat : FeatBase, IArmorProficiency
    {
        public ArmorProficiencyLightFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
            => @"When wearing light armor, the armor check penalty applies only to Balance, Climb, Escape Artist, Hide, Jump, Silent Stealth, Sleight of Hand, and Tumble checks.";

        public override bool MeetsPreRequisite(Creature creature)
        {
            // if not proficient with this armor, then the feat can be added
            return (!creature.Proficiencies.IsProficientWith(ArmorProficiencyType.Light, PowerLevel));
        }

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.Proficiencies.Add(this);
        }

        protected override void OnRemove()
        {
            Creature.Proficiencies.Remove(this);
            base.OnRemove();
        }

        #region IArmorProficiency Members
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
        {
            return (profType <= ArmorProficiencyType.Light) && (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            return IsProficientWith(armor.ProficiencyType, PowerLevel) && (powerLevel >= PowerLevel);
        }

        public string Description
            => @"Light Armor";
        #endregion
    }
}
