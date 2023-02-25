using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable, ItemInfo(@"Slam", @"Damage with Slam Attack", @"")]
    public class Slam : NaturalWeapon
    {
        public Slam(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, bool treatAsSole)
            : base(@"Slam", ItemSlot.SlamSlot, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Slam>(mediumDamage, criticalLow, criticalMultiplier, DamageType.All);
        }

        public Slam(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Slam", ItemSlot.SlamSlot, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Slam>(mediumDamage, criticalLow, criticalMultiplier, damageType);
        }

        public Slam(string initialDamage, Size damageSize, Dictionary<int, Roller> damageRollers, 
            int criticalLow, int criticalMultiplier, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Slam", ItemSlot.SlamSlot, string.Empty, true, damageSize, damageRollers, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Slam>(initialDamage, criticalLow, criticalMultiplier, damageType);
        }

        protected override string ClassIconKey { get { return string.Empty; } }

        public override NaturalWeapon Clone()
        {
            var _rollers = DamageRollers.ToDictionary(_kvp => _kvp.Key, _kvp => _kvp.Value);
            return new Slam(MainHead.MediumDamageRollString, Sizer.NaturalSize, _rollers.Any() ? _rollers : null,
                MainHead.CriticalLow, MainHead.CriticalMultiplier,
                IsPrimary, MainHead.DamageTypes[0], TreatAsSoleWeapon);
        }
    }
}
