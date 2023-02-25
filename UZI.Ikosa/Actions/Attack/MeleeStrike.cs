using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Interactions;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class MeleeStrike : AttackActionBase
    {
        public MeleeStrike(IWeaponHead source, AttackImpact impact, string orderKey)
            : base(source, source.ContainingWeapon.ProvokesMelee, source.ContainingWeapon.ProvokesTarget, orderKey)
        {
            _Impact = impact;
        }

        private AttackImpact _Impact;

        public AttackImpact Impact => _Impact;
        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        public override string Key => @"Melee";
        public override string DisplayName(CoreActor actor)
            => $@"Melee Attack with {WeaponHead.ContainingWeapon.GetKnownName(actor)}";

        #region protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // creature is grappling
                if (_critter.Conditions.Contains(Condition.Grappling))
                {
                    // creature is grappling the target and attack is with a light weapon
                    return new ActivityResponse(MeleeWeapon.IsLightWeapon
                        && _critter.IsGrappling(activity.Targets[0]?.Target?.ID ?? Guid.Empty));
                }

                // regular process
                return base.OnCanPerformActivity(activity);
            }

            // not a creature!
            return new ActivityResponse(false);
        }
        #endregion

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Melee Attack", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(WeaponHead, observer);
            return _obs;
        }

        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.MeleeDeltable.QualifiedValue(
                new Interaction(Weapon?.CreaturePossessor, WeaponHead, null,
                    new MeleeAttackData(null, this, Weapon.GetLocated()?.Locator, Impact, null, false, null, null, 1, 1)));

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Melee.Target", @"Melee Target", Impact,
                WeaponHead.LethalitySelection, MeleeWeapon.HasPenaltyUsingLethalChoice,
                WeaponHead.CriticalLow, null, FixedRange.One, FixedRange.One,
                new StrikeZoneRange(MeleeWeapon), new ObjectTargetType(), new CreatureTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Melee.Target");

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
        }
    }
}
