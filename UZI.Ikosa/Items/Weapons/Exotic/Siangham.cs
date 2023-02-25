using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Siangham", @"Exotic Light 1d6 (x2) Piercing", @"siangham")
    ]
    public class Siangham : MeleeWeaponBase, IExoticWeapon, IWieldMountable
    {
        public Siangham()
            : base("Siangham", WieldTemplate.Light, true)
        {
            Setup(Size.Medium);
        }

        public Siangham(Size creatureSize)
            : base("Siangham", WieldTemplate.Light, true)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<Siangham>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 3m;
            this.BaseWeight = 1d;
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

        protected override string ClassIconKey { get { return @"siangham"; } }
    }
}
