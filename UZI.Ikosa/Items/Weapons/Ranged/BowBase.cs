using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public abstract class BowBase : ProjectileWeaponBase, IActionProvider, IWieldMountable
    {
        protected BowBase(string name, int rangeIncrement, Size itemSize)
            : base(name, rangeIncrement, itemSize)
        {
        }

        public abstract bool CanUseMounted { get; }

        #region public override bool IsActive { get; }
        /// <summary>
        /// Active if slotted in a main slot, 
        /// and either slotted in a secondary or there is holding slot that is either free or using a natural weapon.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                return (MainSlot != null) &&
                    ((SecondarySlot != null) || CreaturePossessor.Body.ItemSlots.AllSlots
                    .Any(_is => _is.SlotType.Equals(ItemSlot.HoldingSlot, StringComparison.OrdinalIgnoreCase)
                        && ((_is.SlottedItem == null) || (_is.SlottedItem is NaturalWeapon))));
            }
        }
        #endregion

        /// <summary>When in use, uses two hands, even if only 1 is slotted (TRUE)</summary>
        public override bool UsesTwoHands => true;

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            var _critter = CreaturePossessor;
            return (from _container in _critter.GetReachable<AmmunitionContainer<Arrow, BowBase>>()
                    from _set in _container.AmmunitionInfos(_critter)
                    let _ammo = _container.GetAmmunition(_critter, _set.InfoIDs)
                    where _ammo != null
                    let _rank = $@"{(1000 - _set.Count):00#}|{_set.Message}"
                    select new BowStrike(new RangedAmmunition(this, _ammo), _container, _rank)).
                    Union(from _bundle in _critter.GetReachable<AmmunitionBundle<Arrow, BowBase>>()
                          from _set in _bundle.AmmunitionInfos(_critter)
                          let _ammo = _bundle.GetAmmunition(_critter, _set.InfoIDs)
                          where _ammo != null
                          let _rank = $@"{(1000 - _set.Count):00#}|{_set.Message}"
                          select new BowStrike(new RangedAmmunition(this, _ammo), _bundle, _rank));
        }

        #region IActionProvider Members

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                var _budget = budget as LocalActionBudget;

                // find strikes that can be performed based on available ammunition
                if (_budget.CanPerformRegular)
                {
                    foreach (var _strk in WeaponStrikes())
                    {
                        yield return new RegularAttack(_strk);
                    }
                }
            }
            yield break;
        }

        #endregion

        #region public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        {
            var _atk = (workSet.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // weapon base damage
            yield return new DamageRollPrerequisite(this, workSet, $@"{keyFix}Weapon", Name,
                WeaponDamageRollers.DiceLookup[MediumDamageRollString][ItemSizer.EffectiveCreatureSize.Order],
                false, _nonLethal, @"Weapon", minGroup);

            // creature-based damage bonuses
            if (CreaturePossessor != null)
                yield return new DamageRollPrerequisite(typeof(Creature), workSet, $@"{keyFix}Creature", @"Creature",
                    new ConstantRoller(CreaturePossessor.ExtraWeaponDamage.QualifiedValue(workSet)),
                    false, _nonLethal, @"Creature", minGroup);
            yield break;
        }
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

        /// <summary>Bows do not naturally get strength damage bonuses</summary>
        public override bool UsesStrengthDamage => false;

        /// <summary>Bows do take strength damage penalties</summary>
        public override bool TakesStrengthDamagePenalty => true;

        public override bool IsTransferrable => true;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);
    }
}
