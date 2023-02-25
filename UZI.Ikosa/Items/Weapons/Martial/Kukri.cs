using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Slashing, 18, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Kukri", @"Martial Light 1d4 (18-20/x2) Slashing", @"kukri")
    ]
    public class Kukri : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Kukri()
            : base("Kukri", WieldTemplate.Light, true)
        {
            Setup(Size.Medium);
        }

        public Kukri(Size creatureSize)
            : base("Kukri", WieldTemplate.Light, true)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<Kukri>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 8m;
            this.BaseWeight = 2d;
            this.MaxStructurePoints.BaseValue = 2;
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

        protected override string ClassIconKey { get { return @"kukri"; } }
    }
}
