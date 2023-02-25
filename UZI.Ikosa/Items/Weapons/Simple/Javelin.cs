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
    [WeaponHead(@"1d6", DamageType.Piercing, 20, 2, typeof(Materials.WoodMaterial), Lethality.NormallyLethal)]
    [ItemInfo(@"Javelin", @"Simple Ranged 1d6 (x2) Piercing (Throw 30')", @"javelin")]
    public class Javelin : MeleeWeaponBase, IThrowableWeapon, IActionProvider, IWieldMountable
    {
        public Javelin()
            : base(@"Javelin", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Javelin>();
            ItemMaterial = Materials.SteelMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            Price.CorePrice = 1m;
            BaseWeight = 2d;
            MaxStructurePoints.BaseValue = 2;
        }

        public override bool IsTransferrable => true;
        public override bool IsActive => MainSlot != null;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        #region IThrowableWeapon Members
        public int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? 60
            : 30;

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

        protected override string ClassIconKey => @"javelin";
    }
}
