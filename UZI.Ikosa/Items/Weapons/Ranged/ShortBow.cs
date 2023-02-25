using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Shortbow", @"Martial Ranged 1d6 (x3) Piercing", @"shortbow")
    ]
    public class ShortBow : BowBase, IMartialWeapon
    {
        public ShortBow()
            : base(@"Shortbow", 60, Size.Small)
        {
            Setup();
        }

        private void Setup()
        {
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 30m;
            this.MaxStructurePoints.BaseValue = 5;
            this.BaseWeight = 2d;
        }

        public override bool CanUseMounted { get { return true; } }

        protected override IWeaponHead GetProxyHead()
        {
            return GetWeaponHead<ShortBow>(true);
        }

        protected override string ClassIconKey { get { return @"shortbow"; } }
    }
}
