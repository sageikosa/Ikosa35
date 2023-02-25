using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Mace, Heavy", @"Simple One-Handed 1d8 (x2) Bludgeoning", @"heavy_mace")
    ]
    public class HeavyMace : MeleeWeaponBase, IWieldMountable
    {
        public HeavyMace()
            : base(@"Mace, Heavy", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<HeavyMace>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.Price.CorePrice = 12m;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.BaseWeight = 8d;
            this.MaxStructurePoints.BaseValue = 20;
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

        protected override string ClassIconKey { get { return @"heavy_mace"; } }
    }
}
