using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), true, Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d6", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Urgosh, Dwarven", @"Exotic Double 1d8/1d6 (x3) Slashing/Piercing", @"urgosh")
    ]
    public class DwarvenUrgosh: DoubleMeleeWeaponBase, IExoticWeapon
    {
        // TODO: double damage if piercing end set against a charge
        public DwarvenUrgosh()
            : base("Urgosh, Dwarven", false, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<DwarvenUrgosh>(true));
            _WeaponHeads.Add(GetWeaponHead<DwarvenUrgosh>(false));
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 50m;
            this.BaseWeight = 12d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        protected override string ClassIconKey { get { return @"urgosh"; } }
    }
}
