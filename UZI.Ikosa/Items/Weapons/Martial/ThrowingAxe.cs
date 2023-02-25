using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Slashing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Axe, Throwing", @"Martial Light 1d6 (x2) Slashing (Throw 10')", @"throwing_axe")
    ]
    public class ThrowingAxe : MeleeWeaponBase, IThrowableWeapon, IMartialWeapon, IWieldMountable
    {
        public ThrowingAxe()
            : base(@"Axe, Throwing", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<ThrowingAxe>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Martial;
            _RangeIncrement = 10;
            Price.CorePrice = 8m;
            BaseWeight = 2d;
            MaxStructurePoints.BaseValue = 2;
        }

        #region IThrowableWeapon Members
        protected int _RangeIncrement;
        public int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? _RangeIncrement * 2
            : _RangeIncrement;

        public virtual int MaxRange => RangeIncrement * 5;
        #endregion

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

        protected override string ClassIconKey { get { return @"throwing_axe"; } }
    }
}
