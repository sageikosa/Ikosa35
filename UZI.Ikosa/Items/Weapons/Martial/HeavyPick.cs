using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 20, 4, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Pick, Heavy", @"Martial One-Handed 1d6 (x4) Piercing", @"heavy_pick")
    ]
    public class HeavyPick : MeleeWeaponBase, IMartialWeapon
    {
        public HeavyPick()
            : base("Pick, Heavy", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<HeavyPick>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 8m;
            this.BaseWeight = 6d;
            this.MaxStructurePoints.BaseValue = 5;
        }

        protected override string ClassIconKey { get { return @"heavy_pick"; } }
    }
}
