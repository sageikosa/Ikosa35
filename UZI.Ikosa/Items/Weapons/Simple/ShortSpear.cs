using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Piercing, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Shortspear", @"Simple One-Handed 1d6 (x2) Piercing (Throw 20')", @"short_spear")
    ]
    public class ShortSpear : MeleeWeaponBase, IThrowableWeapon, IWieldMountable
    {
        public ShortSpear()
            : base("Shortspear", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<ShortSpear>();
           ItemMaterial = Materials.WoodMaterial.Static;
           ProficiencyType = WeaponProficiencyType.Simple;
           _RangeIncrement = 20;
           Price.CorePrice = 1m;
           BaseWeight = 3d;
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

        protected override string ClassIconKey { get { return @"short_spear"; } }
    }
}
