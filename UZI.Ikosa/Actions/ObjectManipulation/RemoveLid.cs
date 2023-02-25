using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Step;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Implemented by a Drop interaction on the lid</summary>
    [Serializable]
    public class RemoveLid : ActionBase
    {
        // TODO: consider a remove to holding action also...

        /// <summary>Implemented by a Drop interaction on the lid</summary>
        public RemoveLid(HollowFurnishingLid lid, ActionTime time, string orderKey)
           : base(lid, time, true, false, orderKey)
        {
        }

        public HollowFurnishingLid HollowFurnishingLid => Source as HollowFurnishingLid;

        public override string Key => @"Lid.Remove";
        public override string DisplayName(CoreActor actor) => @"Remove Lid";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Remove", activity.Actor, observer);
            // TODO: hollow furnishing instead of hollow furnishing lid...
            _obs.Implement = GetInfoData.GetInfoFeedback(HollowFurnishingLid, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity?.Actor is Creature _critter)
            {
                activity.EnqueueRegisterPreEmptively(Budget);

                // not already holding...
                if (_critter.CarryingCapacity.LoadPushDrag < HollowFurnishingLid.Weight)
                {
                    return activity.GetActivityResultNotifyStep(@"Too heavy to move");
                }
                return new RemoveLidStep(activity, HollowFurnishingLid);
            }
            return activity.GetActivityResultNotifyStep(@"Actor not a creature");
        }
    }
}
