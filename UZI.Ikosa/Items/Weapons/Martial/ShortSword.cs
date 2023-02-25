using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Sword, Short", @"Martial Light 1d6 (19-20/x2) Piercing", @"short_sword")
    ]
    public class ShortSword : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public ShortSword()
            : base("Sword, Short", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<ShortSword>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 10m;
            this.BaseWeight = 2d;
            this.MaxStructurePoints.BaseValue = 3; // slightly better than a dagger
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

        protected override string ClassIconKey { get { return @"short_sword"; } }
    }
}
