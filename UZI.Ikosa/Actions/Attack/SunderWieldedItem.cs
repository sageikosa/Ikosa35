using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Sunder used as an attack action against wielded items (weapons/shields)</summary>
    [Serializable]
    public class SunderWieldedItem : AttackActionBase
    {
        #region construction
        /// <summary>Sunder used as an attack action against wielded items (weapons/shields)</summary>
        public SunderWieldedItem(IWeaponHead source, string orderKey)
            : base(source, false, true, orderKey)
        {
            if (WeaponHead.ContainingWeapon.CreaturePossessor.Feats.Contains(typeof(ImprovedSunderFeat)))
            {
                // does not provoke from target if improved
                _Target = false;
                Improved = true;
            }
            else
                Improved = false;
        }
        #endregion

        #region public override int StandardAttackBonus { get; }
        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.OpposedDeltable.QualifiedValue(
                new Interaction(Weapon.CreaturePossessor, WeaponHead, null,
                    new SunderWieldedItemData(null)));
        #endregion

        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        public bool Improved { get; private set; }
        public override string Key => @"Sunder";
        public override string DisplayName(CoreActor actor) 
            => $@"Sunder Wielded Items with {WeaponHead.ContainingWeapon.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Sunder", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
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

                if ((_atkTarget.Target is ISlottedItem _item) && (_item.MainSlot != null))
                {
                    // slotted item, currently slotted
                    if ((_item is ArmorBase)
                        || ((_item is IWeapon) && !(_item as IWeapon).IsSunderable))
                    {
                        // armor and unsunderable weapons cannot be sundered...
                        return activity.GetActivityResultNotifyStep(@"Cannot sunder that");
                    }
                    else if ((_item is IWeapon) || (_item is ShieldBase))
                    {
                        // sundering wielded weapon/shield, use opposed step (attacker's roll already done)
                        return new SunderStep(activity, AttackSource);
                    }
                }

                // normal attack stuff should work fine...already accounts for items and objects
                return new AttackStep(activity, AttackSource);
            }

            return activity.GetActivityResultNotifyStep(@"Insufficient target");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Sunder.Target", @"Sunder Target", AttackImpact.Penetrating,
                Lethality.AlwaysLethal, false, WeaponHead.CriticalLow, null, FixedRange.One, FixedRange.One,
                new StrikeZoneRange(MeleeWeapon), new ObjectTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Sunder.Target");

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
        }
    }
}
