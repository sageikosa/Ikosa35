using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"2d4", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Ranseur", @"Martial Reach Two-Handed 2d4 (x3) Piercing", @"ranseur")
    ]
    public class Ranseur : MeleeWeaponBase, IReachWeapon, IMartialWeapon
    {
        // TODO: +2 Disarm

        public Ranseur()
            : base(@"Ranseur", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Ranseur>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 10m;
            this.BaseWeight = 12d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IReachWeapon Members
        public bool TargetAdjacent { get { return false; } }
        public int ExtraReach { get { return 0; } }
        #endregion

        protected override string ClassIconKey { get { return @"ranseur"; } }
    }
}
