using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    [WeaponHead(@"1d4", DamageType.Piercing, 20, 2, typeof(Materials.WoodMaterial), Lethality.NormallyLethal)]
    [ItemInfo(@"Dart", @"Simple Ranged 1d4 (x2) Piercing (Throw 20')", @"dart")]
    public class Dart : MeleeWeaponBase, IThrowableWeapon, IActionProvider, IWieldMountable
    {
        public Dart()
            : base(@"Dart", WieldTemplate.Light, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Dart>();
            ItemMaterial = Materials.SteelMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            Price.CorePrice = 0.5m;
            BaseWeight = 0.5d;
        }

        public override bool IsTransferrable => true;
        public override bool IsActive => MainSlot != null;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        #region IThrowableWeapon Members
        public int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? 40
            : 20;

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

        protected override string ClassIconKey => @"dart";
    }
}
