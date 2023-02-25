using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d8", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Axe, Orc Double", @"Exotic Double 1d8/1d8 (x3) Slashing", @"double_axe")
    ]
    public class OrcDoubleAxe : DoubleMeleeWeaponBase, IExoticWeapon, IWieldMountable
    {
        public OrcDoubleAxe()
            : base("Axe, Orc Double", false, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<OrcDoubleAxe>(true));
            _WeaponHeads.Add(GetWeaponHead<OrcDoubleAxe>(false));
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 60m;
            this.BaseWeight = 15d;
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

        protected override string ClassIconKey { get { return @"double_axe"; } }
    }
}
