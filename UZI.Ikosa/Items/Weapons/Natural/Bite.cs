using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable, ItemInfo(@"Bite", @"Damage with Bite Attack", @"")]
    public class Bite : NaturalWeapon
    {
        public Bite(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, bool treatAsSole)
            : base(@"Bite", ItemSlot.Mouth, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Bite>(mediumDamage, criticalLow, criticalMultiplier, DamageType.All);
        }

        public Bite(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Bite", ItemSlot.Mouth, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Bite>(mediumDamage, criticalLow, criticalMultiplier, damageType);
        }

        public Bite(string initialDamage, Size damageSize, Dictionary<int, Roller> damageRollers,
            int criticalLow, int criticalMultiplier, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Bite", ItemSlot.Mouth, string.Empty, true, damageSize, damageRollers, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Bite>(initialDamage, criticalLow, criticalMultiplier, damageType);
        }

        protected override string ClassIconKey => string.Empty;

        public override NaturalWeapon Clone()
            => new Bite(MainHead.MediumDamageRollString, Sizer.NaturalSize,
                MainHead.CriticalLow, MainHead.CriticalMultiplier,
                IsPrimary, MainHead.DamageTypes[0], TreatAsSoleWeapon);
    }
}
