using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable, ItemInfo(@"Gore", @"Damage with Horns", @"")]
    public class Gore : NaturalWeapon
    {
        public Gore(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, bool treatAsSole)
            : base(@"Gore", ItemSlot.Horns, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Gore>(mediumDamage, criticalLow, criticalMultiplier, DamageType.PierceAndSlash);
        }

        public Gore(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Gore", ItemSlot.Horns, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Gore>(mediumDamage, criticalLow, criticalMultiplier, damageType);
        }

        protected override string ClassIconKey { get { return string.Empty; } }

        public override NaturalWeapon Clone()
        {
            return new Gore(MainHead.MediumDamageRollString, Sizer.NaturalSize,
                MainHead.CriticalLow, MainHead.CriticalMultiplier,
                IsPrimary, MainHead.DamageTypes[0], TreatAsSoleWeapon);
        }
    }
}
