using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>
    /// This weapon is used to source unarmed attacks
    /// </summary>
    [
    Serializable,
    WeaponHead(@"1d3", DamageType.Bludgeoning, 20, 2, typeof(HideMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Unarmed Weapon", @"Light 1d3 (x2) Bludgeoning", @"")
    ]
    public class ImprovedUnarmedWeapon : MeleeWeaponBase, ITrippingWeapon
    {
        public ImprovedUnarmedWeapon()
            : base(@"Unarmed Strike", WieldTemplate.Unarmed, true, ItemSlot.UnarmedSlot)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<UnarmedWeapon>();
            ItemMaterial = HideMaterial.Static;
            Price.CorePrice = 0;
            ProficiencyType = WeaponProficiencyType.Simple;
            BaseWeight = 0d;
        }

        public override bool IsSunderable => false;
        public override bool IsTransferrable => false;
        public override bool ProvokesTarget => false;
        public override bool OpportunisticAttacks => true;
        public override bool HasPenaltyUsingLethalChoice => false;

        protected override string ClassIconKey
            => nameof(ImprovedUnarmedWeapon);

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return false; }
        }

        #endregion
    }
}
