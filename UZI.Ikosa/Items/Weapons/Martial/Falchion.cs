using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"2d4", DamageType.Slashing, 18, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Falchion", @"Martial Two-Handed 2d4 (18-20/x2) Slashing", @"falchion")
    ]
    public class Falchion : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Falchion()
            : base("Falchion", WieldTemplate.TwoHanded, false)
        {
            Setup(Size.Medium);
        }

        public Falchion(Size creatureSize)
            : base("Falchion", WieldTemplate.TwoHanded, false)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<Falchion>();
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 75m;
            this.BaseWeight = 8d;
            this.MaxStructurePoints.BaseValue = 10;
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

        protected override string ClassIconKey { get { return @"falchion"; } }
    }
}
