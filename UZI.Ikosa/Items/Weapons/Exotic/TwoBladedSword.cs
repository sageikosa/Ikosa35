using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Slashing, 19, 2, typeof(Materials.SteelMaterial), true, Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d8", DamageType.Slashing, 19, 2, typeof(Materials.SteelMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Sword, Two-Bladed", @"Exotic Double 1d8/1d8 (19-20/x2) Slashing", @"two_bladed_sword")
    ]
    public class TwoBladedSword : DoubleMeleeWeaponBase, IExoticWeapon, IWieldMountable
    {
        public TwoBladedSword()
            : base("Sword, Two-Bladed", false, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<TwoBladedSword>(true));
            _WeaponHeads.Add(GetWeaponHead<TwoBladedSword>(false));
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 100m;
            this.BaseWeight = 10d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"two_bladed_sword"; } }
    }
}
