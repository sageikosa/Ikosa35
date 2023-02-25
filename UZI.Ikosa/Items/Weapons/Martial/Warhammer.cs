using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Warhammer", @"Martial One-Handed 1d8 (x3) Bludgeoning", @"warhammer")
    ]
    public class Warhammer : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Warhammer()
            : base("Warhammer", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Warhammer>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 12m;
            this.BaseWeight = 5d;
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

        protected override string ClassIconKey { get { return @"warhammer"; } }
    }
}
