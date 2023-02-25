using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Quarterstaff", @"Simple Double 1d6/1d6 (x2) Bludgeoning", @"quarterstaff")
    ]
    public class Quarterstaff : DoubleMeleeWeaponBase
    {
        public Quarterstaff()
            : base(@"Quarterstaff", false, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<Quarterstaff>(true));
            _WeaponHeads.Add(GetWeaponHead<Quarterstaff>(false));
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 0m;
            this.BaseWeight = 4d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        protected override string ClassIconKey { get { return @"quarterstaff"; } }
    }
}
