using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Trident", @"Martial One-Handed 1d8 (x2) Piercing (Throw 10')", @"trident")
    ]
    public class Trident : MeleeWeaponBase, IThrowableWeapon, IMartialWeapon, IWieldMountable
    {
        public Trident()
            : base(@"Trident", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Trident>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Martial;
            _RangeIncrement = 10;
            Price.CorePrice = 15m;
            BaseWeight = 4d;
            MaxStructurePoints.BaseValue = 5;
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
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"trident"; } }
    }
}
