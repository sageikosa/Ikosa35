using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Slashing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Longsword", @"Martial One-Handed 1d8 (19-20/x2) Slashing", @"long_sword")
    ]
    public class Longsword : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Longsword()
            : base("Longsword", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Longsword>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 15m;
            this.BaseWeight = 4d;
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

        protected override string ClassIconKey { get { return @"long_sword"; } }
    }
}
