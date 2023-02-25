using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable, ItemInfo(@"Tentacle", @"Damage with Tentacle Attack", @"")]
    public class Tentacle : NaturalWeapon
    {
        /// <summary>Default bludgeoning damage</summary>
        public Tentacle(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier,
            string slotType, string subType, bool primary, bool treatAsSole)
            : base(nameof(Tentacle), slotType, subType, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Bite>(mediumDamage, criticalLow, criticalMultiplier, DamageType.Bludgeoning);
        }

        public Tentacle(string mediumDamage, Size damageSize, int criticalLow, int criticalMultiplier,
            string slotType, string subType, bool primary, DamageType damageType, bool treatAsSole)
            : base(nameof(Tentacle), slotType, subType, true, damageSize, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Bite>(mediumDamage, criticalLow, criticalMultiplier, damageType);
        }

        public Tentacle(string initialDamage, Size damageSize, Dictionary<int, Roller> damageRollers, int criticalLow, 
            int criticalMultiplier, string slotType, string subType, bool primary, DamageType damageType, bool treatAsSole)
            : base(nameof(Tentacle), ItemSlot.Tentacle, subType, true, damageSize, damageRollers, primary, true, treatAsSole)
        {
            _MainHead = GetWeaponHead<Slam>(initialDamage, criticalLow, criticalMultiplier, damageType);
        }

        protected override string ClassIconKey => string.Empty;

        public override NaturalWeapon Clone()
            => new Tentacle(MainHead.MediumDamageRollString, Sizer.NaturalSize,
                MainHead.CriticalLow, MainHead.CriticalMultiplier, SlotType, SlotSubType,
                IsPrimary, MainHead.DamageTypes[0], TreatAsSoleWeapon);
    }
}
