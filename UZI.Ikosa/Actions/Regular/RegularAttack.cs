using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class RegularAttack : ActionBase, ISupplyAttackAction
    {
        public RegularAttack(AttackActionBase atk)
            : base(atk, new ActionTime(TimeType.Regular), atk.ProvokesMelee, atk.ProvokesTarget, atk.OrderKey)
        {
        }

        /// <summary>Underlying attack for the action</summary>
        public AttackActionBase Attack => (AttackActionBase)Source;

        public override string Key => $@"Attack.{Attack.Key}";
        public override string DisplayName(CoreActor actor) 
            => $@"Regular Attack as {Attack.DisplayName(actor)}";

        public override bool CombatList
            => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => Attack.GetActivityInfo(activity, observer);

        /// <summary>Overrides default IsHarmless true setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless
            => false;

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            _Budget = budget as LocalActionBudget;
            Attack.CanPerformNow(budget);
            return base.CanPerformNow(budget);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // gets any defined full attack budget (possibly added by multi-weapon disposition)
            var _fullAtkBudget = FullAttackBudget.GetBudget(_Budget);
            if (_fullAtkBudget == null)
            {
                // otherwise set one up
                _fullAtkBudget = new FullAttackBudget(Attack);
                _Budget.BudgetItems.Add(_fullAtkBudget.Source, _fullAtkBudget);
            }

            // attack!
            Attack.SetFullAttackBudget(_fullAtkBudget);
            return Attack.DoAttack(activity);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _mode in Attack.AimingMode(activity))
            {
                yield return _mode;
            }

            yield break;
        }

        #region IWeaponAttackAction Members

        public Items.Weapons.IWeaponHead WeaponHead
            => Attack.WeaponHead;

        public Items.Weapons.IWeapon Weapon
            => Attack.Weapon;

        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => Attack.IsProvocableTarget(activity, potentialTarget);
    }
}
