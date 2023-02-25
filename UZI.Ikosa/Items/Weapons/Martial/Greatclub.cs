using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d10", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Greatclub", @"Martial Two-Handed 1d10 (x2) Bludgeoning", @"great_club")
    ]
    public class Greatclub : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Greatclub()
            : base("Greatclub", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Greatclub>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 5m;
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

        protected override string ClassIconKey { get { return @"great_club"; } }
    }
}
