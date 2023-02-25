using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.LeatherMaterial), Contracts.Lethality.NormallyNonLethal),
    ItemInfo(@"Sap", @"Martial Light 1d6 (x2) Bludgeoning (Non-lethal)", @"sap")
    ]
    public class Sap : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Sap()
            : base("Sap", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Sap>();
            this.ItemMaterial = Materials.ClothMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 1m;
            this.BaseWeight = 2d;
            this.MaxStructurePoints.BaseValue = 2;
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

        protected override string ClassIconKey { get { return @"sap"; } }
    }
}
