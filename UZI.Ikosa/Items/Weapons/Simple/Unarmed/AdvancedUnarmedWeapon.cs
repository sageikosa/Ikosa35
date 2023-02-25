using System;
using System.Collections.Generic;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>
    /// Unarmed weapon used by a monk
    /// </summary>
    [
    Serializable,
    WeaponHead(@"1d3", DamageType.Bludgeoning, 20, 2, typeof(HideMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Unarmed Weapon", @"Light 1d3 (x2) Bludgeoning", @"")
    ]
    public class AdvancedUnarmedWeapon : MeleeWeaponBase, ITrippingWeapon
    {
        public AdvancedUnarmedWeapon(string medium, Dictionary<int, Roller> damageRollers)
            : base(@"Unarmed Strike", WieldTemplate.Unarmed, true, ItemSlot.UnarmedSlot)
        {
            _DamageRollers = damageRollers;
            Setup(medium);
        }

        private void Setup(string medium)
        {
            _MainHead = new WeaponBoundHead<AdvancedUnarmedWeapon>(this, medium, DamageType.Bludgeoning,
                _DamageRollers, 20, 2, ItemMaterial);
            ItemMaterial = HideMaterial.Static;
            Price.CorePrice = 0;
            ProficiencyType = WeaponProficiencyType.Simple;
            BaseWeight = 0d;
        }

        private Dictionary<int, Roller> _DamageRollers = null;

        public override bool IsSunderable => false;
        public override bool IsTransferrable => false;
        public override bool ProvokesTarget => false;
        public override bool OpportunisticAttacks => true;
        public override bool HasPenaltyUsingLethalChoice => false;

        public IEnumerable<KeyValuePair<int, Roller>> DamageRollers
        {
            get
            {
                if (_DamageRollers != null)
                {
                    foreach (var _kvp in _DamageRollers)
                        yield return _kvp;
                }
                yield break;
            }
        }

        protected override string ClassIconKey
            => nameof(AdvancedUnarmedWeapon);

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return false; }
        }

        #endregion
    }
}
