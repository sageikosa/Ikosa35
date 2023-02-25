using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Longbow", @"Martial Ranged 1d8 (x3) Piercing", @"longbow")
    ]
    public class Longbow : BowBase, IMartialWeapon
    {
        public Longbow()
            : base(@"Longbow", 100, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Martial;
            Price.CorePrice = 75m;
            MaxStructurePoints.BaseValue = 6;
            BaseWeight = 3d;
        }

        public override bool CanUseMounted => false;

        protected override IWeaponHead GetProxyHead()
        {
            return GetWeaponHead<Longbow>(true);
        }

        protected override string ClassIconKey => @"longbow"; 
    }
}
