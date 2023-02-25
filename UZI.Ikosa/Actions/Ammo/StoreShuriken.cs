using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class StoreShuriken : ActionBase
    {
        public StoreShuriken(ShurikenGrip grip, string orderKey)
            : base(grip, new ActionTime(Contracts.TimeType.FreeOnTurn), false, false, orderKey)
        {
        }

        public ShurikenGrip ShurikenGrip => ActionSource as ShurikenGrip;

        public override string Key => @"Shuriken.Store";
        public override string DisplayName(CoreActor actor) => @"Store Shuriken";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Store", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(ShurikenGrip.Ammunition[0], observer);
            return _obs;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        private IEnumerable<CoreInfo> Containers(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // nearby containers
                return (from _container in _critter.GetReachable<IAmmunitionTypedBundle<Shuriken, ShurikenGrip>>()
                        let _i = GetInfoData.GetInfoFeedback(_container, _critter)
                        where _i != null
                        select _i).OfType<CoreInfo>();
            }
            return new CoreInfo[] { };
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new CoreListAim(@"AmmoType", @"Ammunition", FixedRange.One, FixedRange.One, Containers(activity));
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                if ((activity.Targets.OfType<ValueTarget<CoreInfo>>().FirstOrDefault())?.Value is AmmoBundleInfo _bundle)
                {
                    // tactically accessible containers?
                    var _storeInto = (from _container in _critter.GetReachable<IAmmunitionTypedBundle<Shuriken, ShurikenGrip>>()
                                      where _container.ID == _bundle.ID
                                      select _container).FirstOrDefault();
                    if (_storeInto != null)
                    {
                        if (ShurikenGrip.Ammunition?.Any() ?? false)
                        {
                            foreach (var _ammo in ShurikenGrip.Ammunition)
                            {
                                // merge and drop any remainder
                                var _store = _ammo.ToAmmunitionBundle(@"Shuriken") as AmmunitionBundle<Shuriken, ShurikenGrip>;
                                var _remainder = _storeInto.Merge(_store);
                                if (_remainder.Count > 0)
                                    Drop.DoDrop(_critter, _remainder, this, true);
                            }
                        }

                        // since all dropping was done on failed merges, clear ammunition in grip, then clear grip
                        ShurikenGrip.Ammunition = null;
                        ShurikenGrip.ClearSlots();
                    }
                }
            }

            // register the activity
            var _register = new RegisterActivityStep(activity, Budget);
            // TODO: following steps if needed
            return _register;
        }
    }
}
