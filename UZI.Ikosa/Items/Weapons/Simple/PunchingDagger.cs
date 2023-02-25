using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Dagger, punching", @"Simple Light 1d4 (x3) Piercing", @"punching_dagger")
    ]
    public class PunchingDagger : MeleeWeaponBase, IWieldMountable
    {
        public PunchingDagger()
            : base("Dagger, Punching", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<PunchingDagger>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 2m;
            this.BaseWeight = 1d; ;
            this.MaxStructurePoints.BaseValue = 2;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"punching_dagger"; } }
    }
}
