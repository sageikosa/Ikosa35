using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Slashing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Kama", @"Exotic Light 1d6 (x2) Slashing", @"kama")
    ]
    public class Kama : MeleeWeaponBase, IExoticWeapon, IWieldMountable, ITrippingWeapon
    {
        public Kama()
            : base(@"Kama", WieldTemplate.Light, true)
        {
            Setup(Size.Medium);
        }

        public Kama(Size creatureSize)
            : base(@"Kama", WieldTemplate.Light, true)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<Kama>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 2m;
            this.BaseWeight = 2d;
            this.MaxStructurePoints.BaseValue = 2;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"kama"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
