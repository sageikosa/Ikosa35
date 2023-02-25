using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 18, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Rapier", @"Martial One-Handed 1d6 (18-20/x2) Piercing", @"rapier")
    ]
    public class Rapier : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Rapier()
            : base("Rapier", WieldTemplate.OneHanded, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Rapier>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 20m;
            this.BaseWeight = 2d;
            this.MaxStructurePoints.BaseValue = 5;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"rapier"; } }
    }
}
