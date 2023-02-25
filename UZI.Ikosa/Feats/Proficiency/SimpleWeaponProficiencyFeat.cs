using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Simple Weapon Proficiency")
    ]
    public class SimpleWeaponProficiencyFeat: FeatBase, IWeaponProficiency
    {
        public SimpleWeaponProficiencyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You make attack rolls with all simple weapons normally."; }
        }

        public override bool MeetsPreRequisite(Creature creature)
        {
            // if creature is not proficienct with simple weapons, then this feat can be added
            return !creature.Proficiencies.IsProficientWith(WeaponProficiencyType.Simple, PowerLevel);
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

        #region IWeaponProficiency Members
        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // true if the normal type not martial and not exotic
            return ((!typeof(IMartialWeapon).IsAssignableFrom(type)) && (!typeof(IExoticWeapon).IsAssignableFrom(type))
                && (powerLevel >= PowerLevel));
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            return (profType == WeaponProficiencyType.Simple)
                && (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            return IsProficientWith(weapon.ProficiencyType, powerLevel);
        }

        public string Description
        {
            get { return @"All simple weapons"; }
        }
        #endregion
    }
}
