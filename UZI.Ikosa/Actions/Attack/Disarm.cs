using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Feats;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Disarm used at an attack action
    /// </summary>
    [Serializable]
    public class Disarm : AttackActionBase, IDamageInterrupts
    {
        #region ctor()
        public Disarm(IWeaponHead source, string orderKey)
            : base(source, false, true, orderKey)
        {
            if (WeaponHead.ContainingWeapon.CreaturePossessor.Feats.Contains(typeof(ImprovedDisarmFeat)))
            {
                // does not provoke from target if improved
                _Target = false;
                Improved = true;
            }
            else
                Improved = false;
        }
        #endregion

        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.OpposedDeltable.QualifiedValue(
                new Interaction(Weapon.CreaturePossessor, WeaponHead, null, new DisarmData(null)));

        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        public bool Improved { get; private set; }
        public override string Key => @"Disarm";
        public override string DisplayName(CoreActor actor)
            => $@"Disarm with {WeaponHead.ContainingWeapon.GetKnownName(actor)}";

        public void Interrupted() { }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Disarm", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(WeaponHead, observer);
            return _obs;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _atkTarget = activity.Targets.OfType<AttackTarget>().FirstOrDefault();
            if ((_atkTarget != null) && (_atkTarget.Target != null))
            {
                // regardless of effect, must record the use of the action
                StandardAttackUseRegistration(activity);

                var _noCounter = WeaponHead.ContainingWeapon.CreaturePossessor.Feats.Contains(typeof(ImprovedDisarmFeat));
                return new DisarmStep(activity, AttackSource, _noCounter);
            }
            return activity.GetActivityResultNotifyStep(@"Insufficient target");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Disarm.Target", @"Disarm Target", AttackImpact.Penetrating,
                Lethality.AlwaysNonLethal, false, WeaponHead.CriticalLow, null, FixedRange.One, FixedRange.One,
                new StrikeZoneRange(MeleeWeapon), new ObjectTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Disarm.Target");

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
        }
    }
}
