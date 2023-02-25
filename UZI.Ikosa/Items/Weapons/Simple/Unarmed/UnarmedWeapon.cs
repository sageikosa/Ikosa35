using System;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>
    /// This weapon is used to source unarmed attacks
    /// </summary>
    [
    Serializable,
    WeaponHead(@"1d3", DamageType.Bludgeoning, 20, 2, typeof(HideMaterial), Contracts.Lethality.NormallyNonLethal),
    ItemInfo(@"Unarmed Weapon", @"Light 1d3 (x2) Bludgeoning", @"")
    ]
    public class UnarmedWeapon : MeleeWeaponBase, ITrippingWeapon
    {
        public UnarmedWeapon()
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
        public override bool ProvokesTarget => true;
        public override bool OpportunisticAttacks => false;

        protected override string ClassIconKey
            => nameof(UnarmedWeapon);

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return false; }
        }

        #endregion
    }
}
