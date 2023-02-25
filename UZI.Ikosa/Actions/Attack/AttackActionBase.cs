using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class AttackActionBase : ActionBase, ISupplyAttackAction
    {
        protected AttackActionBase(IAttackSource source, bool provokesMelee, bool provokesTarget, string orderKey)
            : base(source, new ActionTime(TimeType.SubAction), provokesMelee, provokesTarget, orderKey)
        {
        }

        /// <summary>RegularAttack needs to call this, since the first attack won't have access to the full attack budget yet</summary>
        public void SetFullAttackBudget(FullAttackBudget fullAttack)
            => _AtkBudget ??= fullAttack;

        public IWeaponHead WeaponHead => Source as IWeaponHead;
        public IWeapon Weapon => WeaponHead?.ContainingWeapon;

        public override string WorkshopName => Weapon != null ? DisplayName(Weapon.Possessor) : Key;

        /// <summary>Default AttackSource is the WeaponHead, which typically starts damage workflow</summary>
        public virtual IAttackSource AttackSource => Source as IAttackSource;

        public override bool IsStackBase(CoreActivity activity)
            => false; // attack has no selectable sub-actions as part of its effort

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        /// <summary>Calculated standard attack bonus (may not be one used for target, circumstances and budget)</summary>
        public abstract int StandardAttackBonus { get; }

        private FullAttackBudget _AtkBudget = null;

        /// <summary>Full attack budget (if any) governing this attack.  NULL for Opportunistic Attacks.</summary>
        public FullAttackBudget AttackBudget => _AtkBudget;

        /// <summary>Overrides default IsHarmless setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless
            => false;

        /// <summary>Called when used within another action type (such as regular attack).  Simply a public wrapper for OnPerformActivity.</summary>
        public CoreStep DoAttack(CoreActivity activity)
            => OnPerformActivity(activity);

        /// <summary>Any extra things required by the attack action itself</summary>
        public abstract void AttackResultEffects(AttackResultStep result, Interaction workSet);

        #region protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // if grappling, default is no attack
                if (_critter.Conditions.Contains(Condition.Grappling))
                    return new ActivityResponse(false);

                // regular action base checks
                return base.OnCanPerformActivity(activity);
            }

            // not a creature!
            return new ActivityResponse(false);
        }
        #endregion

        #region public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        /// <summary>Ensures full attack budget is set and the attack action can be performed</summary>
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // NOTE: opportunistic attack does not call this
            _AtkBudget = FullAttackBudget.GetBudget(budget);
            return base.CanPerformNow(budget);
        }
        #endregion

        /// <summary>
        /// Base implementation enqueues a completion step to register weapon use.  
        /// Also enqueues combat disposition step (including two-weapon fighting and standard defensive combat).
        /// </summary>
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            StandardAttackUseRegistration(activity);
            return new AttackStep(activity, AttackSource);
        }

        /// <summary>Can be used by derived classes to simplify registering action budget</summary>
        protected void StandardAttackUseRegistration(CoreActivity activity)
        {
            // NOTE: opportunistic attack does not do this...
            if (_AtkBudget != null)
            {
                // when this attack completes, log the use in the full attack budget
                activity.AppendCompletion(new RegisterWeaponUseStep(activity, _AtkBudget, this));
            }
            activity.EnqueueRegisterPreEmptively(Budget);
        }

        // ISupplyAttackAction
        public AttackActionBase Attack
            => this;

        protected bool IsAttackProvocableTarget(CoreActivity activity, CoreObject potentialTarget, string targetKey)
        {
            if (ProvokesTarget)
            {
                var _loc = potentialTarget?.GetLocated()?.Locator;
                if (_loc != null)
                {
                    foreach (var _target in activity.Targets.OfType<AttackTarget>()
                        .Where(_t => _t.Key.Equals(targetKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (_target.Target.ID == potentialTarget.ID)
                            return true;

                        if ((_target.Target as IAdjunctable)?.GetLocated()?.Locator == _loc)
                            return true;
                    }
                }
            }
            return false;
        }
    }

    public interface ISupplyAttackAction
    {
        AttackActionBase Attack { get; }
    }
}
