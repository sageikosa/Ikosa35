using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Longspear", @"Simple Two-Handed 1d8 (x3) Piercing", @"long_spear")
    ]
    public class Longspear : MeleeWeaponBase, IReachWeapon
    {
        public Longspear()
            : base(@"Longspear", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Longspear>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 5m;
            this.BaseWeight = 9d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IReachWeapon Members
        public bool TargetAdjacent { get { return false; } }
        public int ExtraReach { get { return 0; } }
        #endregion

        protected override string ClassIconKey { get { return @"long_spear"; } }
    }
}
