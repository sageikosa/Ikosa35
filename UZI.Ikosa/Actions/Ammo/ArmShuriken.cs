using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ArmShuriken : ActionBase
    {
        public ArmShuriken(IAmmunitionTypedBundle<Shuriken, ShurikenGrip> bundle, string orderKey)
            : base(bundle, new ActionTime(TimeType.FreeOnTurn), false, false, orderKey)
        {
        }

        public IAmmunitionTypedBundle<Shuriken, ShurikenGrip> ShurikenBundle
            => Source as IAmmunitionTypedBundle<Shuriken, ShurikenGrip>;

        public override string Key => @"Shuriken.Arm";
        public override string DisplayName(CoreActor actor) => $@"Arm Shuriken";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Arm", activity.Actor, observer, GetShuriken(activity));
            _obs.Implement = GetInfoData.GetInfoFeedback(ShurikenBundle, observer);
            return _obs;
        }

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            // must have at least one empty hand
            if ((activity.Actor as Creature)?.Body.ItemSlots[ItemSlot.HoldingSlot, true] != null)
                return base.OnCanPerformActivity(activity);
            return new ActivityResponse(false);
        }

        private Shuriken GetShuriken(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                if (activity.GetFirstTarget<ValueTarget<CoreInfo>>(@"AmmoType")?.Value is AmmoInfo _shuriken)
                {
                    // tactically accessible containers?
                    return ShurikenBundle.GetAmmunition(_critter, _shuriken.InfoIDs);
                }
            }
            return null;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // tactically accessible containers?
                var _ammo = GetShuriken(activity);
                if (_ammo != null)
                {
                    // consume selected ammunition
                    ShurikenBundle.Use(_ammo);

                    var _grip = new ShurikenGrip(_critter, _ammo);
                    var _hand = _critter.Body.ItemSlots.AvailableSlots(_grip).FirstOrDefault();
                    if (_hand != null)
                    {
                        _grip.SetItemSlot(_hand);
                    }
                    else
                    {
                        // didn't grip, so undo consume and possess
                        _grip.Possessor = null;
                        ShurikenBundle.Merge((_ammo, 1));
                    }
                }
            }

            // register the activity
            var _register = new RegisterActivityStep(activity, Budget);
            // TODO: following steps if needed
            return _register;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new CoreListAim(@"AmmoType", @"Ammunition", FixedRange.One, FixedRange.One, AmmoTypes(activity));
            yield break;
        }

        private IEnumerable<CoreInfo> AmmoTypes(CoreActivity activity)
        {
            // sets in bundle
            if (activity.Actor is Creature _critter)
            {
                foreach (var _info in from _set in ShurikenBundle.AmmunitionInfos(_critter)
                                      select _set)
                {
                    yield return _info;
                }
            }
            yield break;
        }
    }
}
