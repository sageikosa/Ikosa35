using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Mace, Light", @"Simple Light 1d6 (x2) Bludgeoning", @"light_mace")
    ]
    public class LightMace : MeleeWeaponBase, IWieldMountable
    {
        public LightMace()
            : base(@"Mace, Light", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<LightMace>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.Price.CorePrice = 5m;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.BaseWeight = 4d;
            this.MaxStructurePoints.BaseValue = 10;
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

        protected override string ClassIconKey { get { return @"light_mace"; } }
    }
}
