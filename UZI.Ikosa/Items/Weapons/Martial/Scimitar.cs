using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Slashing, 18, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Scimitar", @"Martial One-Handed 1d6 (18-20/x2) Slashing", @"scimitar")
    ]
    public class Scimitar : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Scimitar()
            : base("Scimitar", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Scimitar>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 15m;
            this.BaseWeight = 4d;
            this.MaxStructurePoints.BaseValue = 5;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"scimitar"; } }
    }
}
