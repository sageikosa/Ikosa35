using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d10", new DamageType[] { DamageType.Piercing, DamageType.Slashing },
        20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Halberd", @"Martial Two-Handed 1d10 (x3) Bludgeoning and Piercing", @"halberd")
    ]
    public class Halberd : MeleeWeaponBase, IMartialWeapon, ITrippingWeapon
    {
        public Halberd()
            : base(@"Halberd", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Halberd>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 10m;
            this.BaseWeight = 12d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        protected override string ClassIconKey { get { return @"halberd"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
