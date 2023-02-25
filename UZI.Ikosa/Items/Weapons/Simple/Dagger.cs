using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", new DamageType[] { DamageType.Slashing, DamageType.Piercing }, 19, 2,
        typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Dagger", @"Simple Light 1d4 (19-20/x2) Piercing or Slashing (Throw 10')", @"dagger")
    ]
    public class Dagger : MeleeWeaponBase, IThrowableWeapon, IWieldMountable
    {
        public Dagger()
            : base("Dagger", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Dagger>();
            ItemMaterial = Materials.SteelMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            _RangeIncrement = 10;
            Price.CorePrice = 2m;
            BaseWeight = 1d;
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
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"dagger"; } }
    }
}
