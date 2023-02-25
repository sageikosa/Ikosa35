using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PutLid : ActionBase
    {
        public PutLid(HollowFurnishingLid lid, ActionTime time, string orderKey)
            : base(lid, time, true, false, orderKey)
        {
        }

        public HollowFurnishingLid HollowFurnishingLid => Source as HollowFurnishingLid;

        public override string Key => @"Lid.Put";
        public override string DisplayName(CoreActor actor) => @"Put Lid on Object";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Put", activity.Actor, observer);
            _obs.Implement = GetInfoData.GetInfoFeedback(HollowFurnishingLid, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // NOTE: not checking lid validity at this time, don't want to give anything away
            yield return new ObjectListAim(@"Object", @"Object to Seal", FixedRange.One, FixedRange.One,
                PickUp.PickUpList(activity?.Actor as Creature));
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity?.Actor is Creature _critter)
            {
                // TODO: 
                activity.EnqueueRegisterPreEmptively(Budget);

                // NOTE: if target is not a hollow furnishing the PutLidStep will do nothing
                var _furnish = (activity.Targets.OfType<AimTarget>()?.FirstOrDefault())?.Target as HollowFurnishing;
                return new PutLidStep(activity, HollowFurnishingLid, _furnish);
            }

            return activity.GetActivityResultNotifyStep(@"Actor not a creature");
        }
    }
}
