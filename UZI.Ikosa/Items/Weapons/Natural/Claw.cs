using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable, ItemInfo(@"Claw", @"Damage with creature's claw", @"")]
    public class Claw : NaturalWeapon
    {
        public Claw(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, string slotSubType, bool primary, bool treatAsSole)
            : base(@"Claw", ItemSlot.HoldingSlot, slotSubType, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Claw>(mediumDamage, criticalLow, criticalMultiplier, DamageType.PierceAndSlash);
        }

        public Claw(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier, string slotSubType, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Claw", ItemSlot.HoldingSlot, slotSubType, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Claw>(mediumDamage, criticalLow, criticalMultiplier, damageType);
        }

        public Claw(string initialDamage, Size damageSize, Dictionary<int, Roller> damageRollers,
            int criticalLow, int criticalMultiplier, string slotSubType, bool primary, DamageType damageType, bool treatAsSole)
            : base(@"Claw", ItemSlot.HoldingSlot, slotSubType, true, damageSize, damageRollers, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Claw>(initialDamage, criticalLow, criticalMultiplier, damageType);
        }

        protected override string ClassIconKey { get { return string.Empty; } }

        public override NaturalWeapon Clone()
        {
            return new Claw(MainHead.MediumDamageRollString, Sizer.NaturalSize,
                MainHead.CriticalLow, MainHead.CriticalMultiplier, this.SlotSubType,
                IsPrimary, MainHead.DamageTypes[0], TreatAsSoleWeapon);
        }
    }
}
