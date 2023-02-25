using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"2d6", DamageType.Slashing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Greatsword", @"Martial Two-Handed 2d6 (19-20/x2) Slashing", @"great_sword")
    ]
    public class Greatsword : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Greatsword()
            : base("Greatsword", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Greatsword>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 50m;
            this.BaseWeight = 8d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"great_sword"; } }
    }
}
