using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    /// <summary>
    /// Base class for a reloadable crossbow.
    /// </summary>
    [Serializable]
    public abstract class CrossbowBase : ProjectileWeaponBase, IActionProvider
    {
        #region Construction
        protected CrossbowBase(string name, int range, Size itemSize)
            : base(name, range, itemSize)
        {
            _Ammo = null;
        }
        #endregion

        protected CrossbowBolt[] _Ammo;

        /// <summary>Indicates the crossbow is ready to strike</summary>
        public bool IsLoaded => (_Ammo?.Length ?? 0) > 0;

        /// <summary>Indicates the crossbow has more than one bolt loaded (which will prevent regular attacks)</summary>
        public bool IsOverloaded => (_Ammo?.Length ?? 0) > 1;

        /// <summary>When in use, uses two hands, even if only 1 is slotted (FALSE)</summary>
        public override bool UsesTwoHands => false;

        public override bool IsTransferrable => true;

        public CrossbowBolt[] Ammunition { get { return _Ammo; } set { _Ammo = value; } }
        public abstract TimeType ReloadAction { get; }
        public override bool IsActive => MainSlot != null;

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            if (IsLoaded && !IsOverloaded)
            {
                var _rAmmo = new RangedAmmunition(this, Ammunition[0]);
                yield return new CrossbowStrike(_rAmmo, @"101");
            }
            yield break;
        }

        #region IActionProvider Members
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // get budget info
            var _budget = budget as LocalActionBudget;

            // reload?
            var _rlAct = ReloadAction;
            if ((_rlAct == TimeType.Free)
                || ((_rlAct == TimeType.Brief) && _budget.CanPerformBrief)
                || ((_rlAct == TimeType.Total) && _budget.CanPerformTotal))
            {
                yield return new ReloadCrossbow(this, @"201");
            }

            if (IsActive)
            {
                // TODO: special weapon abilities

                if (_budget.CanPerformRegular)
                {
                    foreach (var _strike in WeaponStrikes())
                        yield return new RegularAttack(_strike);
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

        /// <summary>Crossbows do not rely on strength</summary>
        public override bool UsesStrengthDamage => false;

        /// <summary>Crossbows are not penalized by low strength</summary>
        public override bool TakesStrengthDamagePenalty => false;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ToInfo<LoadableProjectileWeaponInfo>(actor, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            if (fetchedInfo is LoadableProjectileWeaponInfo _loadable)
            {
                _loadable.LoadedAmmunition = _Ammo?.Select(_a => GetInfoData.GetInfoFeedback(_a, actor)).ToList() ?? new List<Info>();
            }
            return fetchedInfo;
        }
    }
}
