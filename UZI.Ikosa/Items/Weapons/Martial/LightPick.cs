using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Piercing, 20, 4, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Pick, Light", @"Martial Light 1d4 (x4) Piercing", @"light_pick")
    ]
    public class LightPick : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public LightPick()
            : base("Pick, Light", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<LightPick>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 4m;
            this.BaseWeight = 3d;
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

        protected override string ClassIconKey { get { return @"light_pick"; } }
    }
}
