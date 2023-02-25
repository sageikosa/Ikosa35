using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Hammer, Light", @"Martial Light 1d4 (x2) Bludgeoning (Throw 20')", @"light_hammer")
    ]
    public class LightHammer : MeleeWeaponBase, IThrowableWeapon, IMartialWeapon, IWieldMountable
    {
        public LightHammer()
            : base("Hammer, Light", WieldTemplate.Light, true)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<LightHammer>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Martial;
            _RangeIncrement = 20;
            Price.CorePrice = 1m;
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

        protected override string ClassIconKey => @"light_hammer"; 
    }
}
