using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ReleaseOneHand : ActionBase
    {
        public ReleaseOneHand(HoldingSlot slot, string orderKey)
            : base(slot, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
        }

        public HoldingSlot ItemSlot => Source as HoldingSlot;
        public override string Key => @"ReleaseOneHand";
        public override string DisplayName(CoreActor actor)
        {
            var _info = GetInfoData.GetInfoFeedback(ItemSlot.SlottedItem as CoreObject, ItemSlot.Creature) as ObjectInfo;
            var _title = _info?.Message ?? @"Held Object";
            return $@"Release {_title} from {ItemSlot.ActionName}";
        }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Release", activity.Actor, observer, ItemSlot.SlottedItem as CoreObject);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (ItemSlot.SlottedItem?.MainSlot == ItemSlot)
            {
                // slot to just secondary
                ItemSlot.SlottedItem?.SetItemSlot(ItemSlot.SlottedItem?.SecondarySlot);
            }
            else
            {
                // slot to just primary
                ItemSlot.SlottedItem?.SetItemSlot(ItemSlot.SlottedItem?.MainSlot);
            }
            activity.EnqueueRegisterPreEmptively(Budget);

            // feedback
            var _step = activity.GetActivityResultNotifyStep(@"Release Item from One Hand");
            _step.AppendFollowing(activity.GetNotifyStep(
                new RefreshNotify(true, false, false, true, false)));
            return _step;
        }
    }
}
