using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d12", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Lethality.NormallyLethal),
    ItemInfo(@"Greataxe", @"Martial Two-Handed 1d12 (x3) Slashing", @"great_axe")
    ]
    public class Greataxe : MeleeWeaponBase, IMartialWeapon, IWieldMountable
    {
        public Greataxe()
            : base(@"Greataxe", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Greataxe>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Martial;
            Price.CorePrice = 20m;
            BaseWeight = 12d;
            MaxStructurePoints.BaseValue = 10;
        }

        public IEnumerable<string> SlotTypes
            => ItemSlot.LargeWieldMount.ToEnumerable();

        protected override string ClassIconKey => @"great_axe"; 
    }
}
