using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class AdjustGear : ActionBase
    {
        public AdjustGear(ActionTime actionTime, string orderKey)
            : base(null, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.AdjustGear";
        public override string DisplayName(CoreActor actor) => @"Adjust Gear";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Adjusting Gear", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => true;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return null;
        }
    }
}
