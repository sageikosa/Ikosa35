using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d10", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Glaive", @"Martial Two-Handed 1d10 (x3) Slashing", @"glaive")
    ]
    public class Glaive : MeleeWeaponBase, IReachWeapon, IMartialWeapon
    {
        public Glaive()
            : base(@"Glaive", WieldTemplate.TwoHanded, false)
        {
            Setup(Size.Medium);
        }

        public Glaive(Size creatureSize)
            : base(@"Glaive", WieldTemplate.TwoHanded, false)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<Glaive>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 8m;
            this.BaseWeight = 10d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IReachWeapon Members
        public bool TargetAdjacent { get { return false; } }
        public int ExtraReach { get { return 0; } }
        #endregion

        protected override string ClassIconKey { get { return @"glaive"; } }
    }
}
