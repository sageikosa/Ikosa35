using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Battleaxe", @"Martial One-Handed 1d8 (x2) Bludgeoning and Piercing", @"battle_axe")
    ]
    public class BattleAxe : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public BattleAxe()
            : base("Battleaxe", WieldTemplate.OneHanded, false)
        {
            Setup(Size.Medium);
        }

        public BattleAxe(Size creatureSize)
            : base("Battleaxe", WieldTemplate.OneHanded, false)
        {
            Setup(creatureSize);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<BattleAxe>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 10m;
            this.BaseWeight = 6d;
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

        protected override string ClassIconKey { get { return @"battle_axe"; } }
    }
}
