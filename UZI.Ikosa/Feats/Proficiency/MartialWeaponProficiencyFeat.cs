using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    ParameterizedFeatInfo(@"Martial Weapon Proficiency", @"Cancels -4 penalty with specific martial weapon", 
        typeof(MartialWeaponsLister)),
    ]
    public class MartialWeaponProficiencyFeat<Wpn>: FeatBase, IWeaponProficiency where Wpn: IMartialWeapon
    {
        public MartialWeaponProficiencyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Name
        {
            get { return string.Format(@"Martial Weapon Proficiency ({0})", typeof(Wpn).Name); }
        }

        public override string Benefit
        {
            get { return string.Format(@"You make attack rolls with the martial weapon '{0}' normally.", typeof(Wpn).Name); }
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
            return (type == typeof(Wpn)) && (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            // this feat is for a specific type, so martial in general is right out
            return false;
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            // this feat doesn't care about instances, only specific types
            return IsProficientWithWeapon(weapon.GetType(), powerLevel);
        }

        public string Description
        {
            get { return ItemBase.GetInfo(typeof(Wpn)).Name; }
        }

        #endregion
    }
}
