using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d6", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Club", @"Simple One-Handed 1d6 (x2) Bludgeoning (Throw 10')", @"club")
    ]
    public class Club : MeleeWeaponBase, IThrowableWeapon, IWieldMountable
    {
        public Club()
            : base(@"Club", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Club>();
            ItemMaterial = Materials.WoodMaterial.Static;
            Price.CorePrice = 0m;
            ProficiencyType = WeaponProficiencyType.Simple;
            _RangeIncrement = 10;
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
                yield return ItemSlot.WieldMount;
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"club"; } }
    }
}
