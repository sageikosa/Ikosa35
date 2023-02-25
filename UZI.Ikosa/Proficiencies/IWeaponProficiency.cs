using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public interface IWeaponProficiency
    {
        /// <summary>Typically implemented by classes and races to indicate simple/martial proficiencies.</summary>
        bool IsProficientWith(WeaponProficiencyType profType, int powerLevel);

        /// <summary>Implementers may want to check for specific type and/or the IMartialWeapon and IExoticWeapon interfaces.</summary>
        bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon;

        /// <summary>Typically a weapon will add a specific proficiency due to an enhancement when wielded.</summary>
        bool IsProficientWith(IWeapon weapon, int powerLevel);

        /// <summary>should work the same as the generic version</summary>
        bool IsProficientWithWeapon(Type type, int powerLevel);

        string Description { get; }
    }

    public interface IWeaponProficiencyTreatment : IWeaponProficiency
    {
        /// <summary>Which proficiency class this weapon type is treated as</summary>
        WeaponProficiencyType WeaponTreatment(Type weaponType, int powerLevel);

        /// <summary>Which proficiency class this weapon is treated as</summary>
        WeaponProficiencyType WeaponTreatment(IWeapon weapon, int powerLevel);
    }
}
