using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Sai", @"Exotic Light 1d4 (x2) Bludgeoning", @"sai")
    ]
    public class Sai : MeleeWeaponBase, IThrowableWeapon, IExoticWeapon, IWieldMountable
    {
        // TODO: +4 disarm

        public Sai()
            : base("Sai", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Sai>();
            ItemMaterial = Materials.SteelMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Exotic;
            _RangeIncrement = 10;
            Price.CorePrice = 1m;
            BaseWeight = 1d;
            MaxStructurePoints.BaseValue = 5; // stronger than a blade, but weaker than a mace
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

        protected override string ClassIconKey => @"sai";
    }
}
