using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.BludgeonAndPierce, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Morningstar", @"Simple One-Handed 1d8 (x2) Bludgeoning and Piercing", @"morning_star")
    ]
    public class MorningStar : MeleeWeaponBase, IWieldMountable
    {
        public MorningStar()
            : base(@"MorningStar", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<MorningStar>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.Price.CorePrice = 8;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.BaseWeight = 6d;
            this.MaxStructurePoints.BaseValue = 5;
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

        protected override string ClassIconKey { get { return @"morning_star"; } }
    }
}
