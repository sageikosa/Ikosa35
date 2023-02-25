using System;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo("Armor Proficiency (Heavy)")
    ]
    public class ArmorProficiencyHeavyFeat : FeatBase, IArmorProficiency
    {
        public ArmorProficiencyHeavyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
            => "When wearing heavy armor, the armor check penalty applies only to Balance, Climb, Escape Artist, Hide, Jump, Silent Stealth, Sleight of Hand, and Tumble checks.";

        public override bool MeetsPreRequisite(Creature creature)
        {
            // proficient with medium, but not heavy armor
            return (creature.Proficiencies.IsProficientWith(ArmorProficiencyType.Medium, PowerLevel)
                && (!creature.Proficiencies.IsProficientWith(ArmorProficiencyType.Heavy, PowerLevel)));
        }

        public override string PreRequisite
            => @"Proficiency with Medium Armor";

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
            // heavy is the creme-de-la-creme of armor proficiencies
            return (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            // heavy is the creme-de-la-creme of armor proficiencies
            return (powerLevel >= PowerLevel);
        }

        public string Description
            => @"Heavy Armor";
        #endregion
    }
}
