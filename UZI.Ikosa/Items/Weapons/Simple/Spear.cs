using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Spear", @"Simple Two-Handed 1d8 (x3) Piercing (Throw 20')", @"spear")
    ]
    public class Spear : MeleeWeaponBase, IThrowableWeapon
    {
        public Spear()
            : base(@"Spear", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Spear>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            Price.CorePrice = 2m;
            BaseWeight = 6d;
            _RangeIncrement = 20;
            MaxStructurePoints.BaseValue = 10;
        }

        #region IThrowableWeapon Members
        protected int _RangeIncrement;
        public int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? _RangeIncrement * 2
            : _RangeIncrement;

        public virtual int MaxRange => RangeIncrement * 5;
        #endregion

        protected override string ClassIconKey { get { return @"spear"; } }
    }
}
