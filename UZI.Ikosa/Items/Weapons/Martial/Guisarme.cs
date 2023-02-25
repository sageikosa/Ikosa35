using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"2d4", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Guisarme", @"Martial Two-Handed 2d4 (x3) Slashing", @"guisarme")
    ]
    public class Guisarme : MeleeWeaponBase, IReachWeapon, IMartialWeapon, ITrippingWeapon
    {
        public Guisarme()
            : base(@"Guisarme", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Guisarme>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 9m;
            this.BaseWeight = 12d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IReachWeapon Members
        public bool TargetAdjacent { get { return false; } }
        public int ExtraReach { get { return 0; } }
        #endregion

        protected override string ClassIconKey { get { return @"guisarme"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
