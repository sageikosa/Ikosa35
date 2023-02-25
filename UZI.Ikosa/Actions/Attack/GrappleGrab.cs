using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used as an attack to start a grapple.</summary>
    [Serializable]
    public class GrappleGrab : AttackActionBase
    {
        /// <summary>Used as an attack to start a grapple.</summary>
        public GrappleGrab(IWeaponHead source, bool provokes, string orderKey)
            : base(source, false, provokes, orderKey)
        {
            // TODO: touch attack
        }

        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        public override string Key => nameof(GrappleGrab);
        public override string DisplayName(CoreActor observer)
            => @"Attempt to grab";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Grab Attempt", activity.Actor, observer, 
                activity.Targets[0].Target as CoreObject);
            return _obs;
        }

        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.OpposedDeltable.QualifiedValue(
                new Interaction(Weapon?.CreaturePossessor, WeaponHead, null,
                    new MeleeAttackData(null, this, Weapon.GetLocated()?.Locator, AttackImpact.Touch, 
                        null, false, null, null, 1, 1)));

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new TouchAim(@"Melee.Touch", @"Grab Target", Lethality.AlwaysNonLethal, 20,
                null, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Melee.Touch");

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            StandardAttackUseRegistration(activity);
            if (activity?.Actor is Creature _critter)
            {
                if (_critter.HealthPoints.CurrentValue < InitialHealthPoints)
                {
                    // took damage since action started
                    return activity.GetActivityResultNotifyStep(@"Took damage while attempting");
                }
                else
                {
                    // TODO: melee touch attack resolution
                }
            }
            return null;
        }

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
            // TODO: opposed grapple: success ? damage : done
            // TODO: move into target's space: move ? grapple adjunct : lose hold
        }
    }
}
