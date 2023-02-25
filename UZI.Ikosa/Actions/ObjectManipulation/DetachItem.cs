using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DetachItem : ActionBase
    {
        public DetachItem(AttachmentSlot attachment, ActionTime timeCost, string orderKey)
            : this(attachment, timeCost, true, orderKey)
        {
        }

        public DetachItem(AttachmentSlot attachment, ActionTime timeCost, bool provokesMelee, string orderKey)
            : base(attachment, timeCost, provokesMelee, false, orderKey)
        {
        }

        public AttachmentSlot AttachmentSlot
            => ActionSource as AttachmentSlot;

        public override string Key => @"Item.Detach";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => (TimeCost.ActionTimeType == TimeType.Free)
            ? false
            : base.WillClearStack(budget, activity);

        public override string DisplayName(CoreActor actor)
            => $@"Detach {AttachmentSlot.SlottedItem.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory
                .CreateInfo(@"Detaching Item", activity.Actor, observer, AttachmentSlot.SlottedItem.BaseObject);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        // TODO: ensure detachment is allowed (self, friendly or helpless attendee)

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return new DetachItemStep(activity, this);
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    [Serializable]
    public class DetachItemStep : CoreStep
    {
        public DetachItemStep(CoreActivity activity, DetachItem detach)
            : base(activity)
        {
            _Detach = detach;
        }

        #region state
        private DetachItem _Detach;
        #endregion

        public CoreActivity Activity => Process as CoreActivity;
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsNewRoot => false;

        protected override bool OnDoStep()
        {
            if (Activity.Actor is Creature _critter
                && ManipulateTouch.CanManipulateTouch(_critter, _Detach.AttachmentSlot.SlottedItem.BaseObject))
            {
                var _purloin = new Purloin(Activity.Actor, (Activity.Action as ActionBase)?.TimeCost);
                var _workSet = new Interaction(Activity.Actor, this, _Detach.AttachmentSlot.SlottedItem.BaseObject, _purloin);
                _Detach.AttachmentSlot.SlottedItem.BaseObject.HandleInteraction(_workSet);
            }
            else
            {
                Activity.Terminate(@"Cannot touch");
            }
            return true;
        }
    }
}
