using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 20, 3, typeof(Materials.WoodMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Lance", @"Martial Reach Two-Handed 1d8 (x3) Piercing", @"light_lance")
    ]
    public class Lance : MeleeWeaponBase, IReachWeapon, IMartialWeapon
    {
        // TODO: special mount options (one-hand wielding and double damage)

        public Lance()
            : base(@"Lance", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Lance>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 10m;
            this.BaseWeight = 10d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IReachWeapon Members
        public bool TargetAdjacent { get { return false; } }
        public int ExtraReach { get { return 0; } }
        #endregion

        protected override string ClassIconKey { get { return @"light_lance"; } }
    }
}
