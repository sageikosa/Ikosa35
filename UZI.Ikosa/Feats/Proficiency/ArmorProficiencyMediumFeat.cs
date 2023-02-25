using System;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Armor Proficiency (Medium)")
    ]
    public class ArmorProficiencyMediumFeat : FeatBase, IArmorProficiency
    {
        public ArmorProficiencyMediumFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
            => @"When wearing medium armor, the armor check penalty applies only to Balance, Climb, Escape Artist, Hide, Jump, Silent Stealth, Sleight of Hand, and Tumble checks.";

        public override string PreRequisite => @"Proficiency with Light Armor";

        public override bool MeetsPreRequisite(Creature creature)
        {
            // proficient with light, but not medium armor
            return (creature.Proficiencies.IsProficientWith(ArmorProficiencyType.Light, PowerLevel)
                && (!creature.Proficiencies.IsProficientWith(ArmorProficiencyType.Medium, PowerLevel)));
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
            => (profType <= ArmorProficiencyType.Medium)
            && (powerLevel >= PowerLevel);

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => IsProficientWith(armor.ProficiencyType, powerLevel);

        public string Description
            => @"Medium Armor";
        #endregion
    }
}
