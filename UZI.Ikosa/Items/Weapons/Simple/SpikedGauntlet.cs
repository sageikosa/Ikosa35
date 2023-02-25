using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Piercing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Gauntlet, spiked", @"Simple Light 1d4 (x2) Piercing", @"spiked_gauntlet")
    ]
    public class SpikedGauntlet : MeleeWeaponBase
    {
        // TODO: disarm immunity

        public SpikedGauntlet()
            : base("Gauntlet, Spiked", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<SpikedGauntlet>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 5m;
            this.BaseWeight = 1d;
            this.MaxStructurePoints.BaseValue = 10;
            _SlotType = ItemSlot.HandsSlot;
        }

        protected override string ClassIconKey { get { return @"spiked_gauntlet"; } }
    }
}
