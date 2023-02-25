using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d3", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Gauntlet", @"Simple Unarmed 1d3 (x2) Bludgeoning", @"gauntlet")
    ]
    public class Gauntlet : MeleeWeaponBase
    {
        public Gauntlet()
            : base("Gauntlet", WieldTemplate.Unarmed, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Gauntlet>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 2m;
            this.BaseWeight = 1d;
            this.MaxStructurePoints.BaseValue = 10;
            _SlotType = ItemSlot.HandsSlot;
        }

        protected override string ClassIconKey { get { return string.Empty; } }
    }
}
