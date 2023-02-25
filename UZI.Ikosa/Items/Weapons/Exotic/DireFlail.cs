using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), true, Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Flail, Dire", @"Exotic Double 1d8/1d8 (x2) Bludgeoning", @"dire_flail")
    ]
    public class DireFlail : DoubleMeleeWeaponBase, IExoticWeapon, IWieldMountable, ITrippingWeapon
    {
        // TODO: disarm +2 and trip
        public DireFlail()
            : base("Flail, Dire", false, Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<DireFlail>(true));
            _WeaponHeads.Add(GetWeaponHead<DireFlail>(false));
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 90m;
            this.BaseWeight = 10d;
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

        protected override string ClassIconKey { get { return @"dire_flail"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
