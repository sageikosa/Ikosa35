using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Nunchaku", @"Exotic Light 1d6 (x2) Bludgeoning and Piercing", @"nunchaku")
    ]
    public class Nunchaku : MeleeWeaponBase, IExoticWeapon, IWieldMountable
    {
        // TODO: +2 disarm

        public Nunchaku()
            : base("Nunchaku", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Nunchaku>();
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

        protected override string ClassIconKey { get { return @"nunchaku"; } }
    }
}
