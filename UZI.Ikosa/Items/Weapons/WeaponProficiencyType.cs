using System;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    public static class WeaponProficiencyHelper
    {
        public static WeaponProficiencyType StandardType(Type type)
        {
            if (typeof(NaturalWeapon).IsAssignableFrom(type))
                return WeaponProficiencyType.Natural;
            if (typeof(IExoticWeapon).IsAssignableFrom(type))
                return WeaponProficiencyType.Exotic;
            if (typeof(IMartialWeapon).IsAssignableFrom(type))
                return WeaponProficiencyType.Martial;
            return WeaponProficiencyType.Simple;
        }
    }

    public interface IMartialWeapon: IWeapon
    {
    }

    public interface IExoticWeapon: IWeapon
    {
    }

    public interface IExoticWeaponHeavy: IExoticWeapon
    {
    }
}