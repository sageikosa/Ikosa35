using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class WeaponHeadAttribute: Attribute
    {
        public WeaponHeadAttribute(string mediumDamage, DamageType damageType, 
            int criticalLow, int criticalMultiplier, Type material, 
            Lethality lethality)
        {
            MediumDamage = mediumDamage;
            DamageTypes = new DamageType[] { damageType };
            CriticalLow = criticalLow;
            CriticalMultiplier = criticalMultiplier;
            HeadMaterial = (Materials.Material)material.GetField("Static").GetValue(null);
            Main = true;
            Lethality = lethality;
        }

        public WeaponHeadAttribute(string mediumDamage, DamageType[] damageTypes,
            int criticalLow, int criticalMultiplier, Type material,
            Lethality lethality)
        {
            MediumDamage = mediumDamage;
            DamageTypes = damageTypes;
            CriticalLow = criticalLow;
            CriticalMultiplier = criticalMultiplier;
            HeadMaterial = (Materials.Material)material.GetField("Static").GetValue(null);
            Main = true;
            Lethality = lethality;
        }

        public WeaponHeadAttribute(string mediumDamage, DamageType damageType,
            int criticalLow, int criticalMultiplier, Type material, bool main,
            Lethality lethality)
        {
            MediumDamage = mediumDamage;
            DamageTypes = new DamageType[] { damageType };
            CriticalLow = criticalLow;
            CriticalMultiplier = criticalMultiplier;
            HeadMaterial = (Materials.Material)material.GetField("Static").GetValue(null);
            Main = main;
            Lethality = lethality;
        }

        public WeaponHeadAttribute(string mediumDamage, DamageType[] damageTypes,
            int criticalLow, int criticalMultiplier, Type material, bool main,
            Lethality lethality)
        {
            MediumDamage = mediumDamage;
            DamageTypes = damageTypes;
            CriticalLow = criticalLow;
            CriticalMultiplier = criticalMultiplier;
            HeadMaterial = (Materials.Material)material.GetField("Static").GetValue(null);
            Main = main;
            Lethality = lethality;
        }

        public readonly string MediumDamage;
        public readonly DamageType[] DamageTypes;
        public readonly int CriticalLow;
        public readonly int CriticalMultiplier;
        public readonly Materials.Material HeadMaterial;
        public readonly bool Main;
        public readonly Lethality Lethality;
    }
}
