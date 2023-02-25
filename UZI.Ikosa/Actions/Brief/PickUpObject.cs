using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PickUpObject : ActionBase
    {
        public PickUpObject(HoldingSlot source, string orderKey)
            : base(source, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public HoldingSlot HoldingSlot => Source as HoldingSlot;
        public override string Key => @"Object.PickUp";
        public override string DisplayName(CoreActor actor) => @"Pick up an object";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Pick up", activity.Actor, observer, activity.Targets[0].Target as ICoreObject);

        protected override CoreStep OnPerformActivity(Core.CoreActivity activity)
        {
            if (activity.Targets[0].Target is ICoreObject _target)
            {
                activity.EnqueueRegisterPreEmptively(Budget);
                return new PickUpObjectStep(activity, HoldingSlot, _target);
            }
            return activity.GetActivityResultNotifyStep(@"Nothing selected");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new ObjectListAim(@"Object", @"Object to pick up", FixedRange.One, FixedRange.One, PickUp.PickUpList(HoldingSlot.Creature));
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}