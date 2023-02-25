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
    public class HoldSecondSlot : ActionBase
    {
        public HoldSecondSlot(HoldingSlot slot, ISlottedItem item, string orderKey)
            : base(slot, new ActionTime(Contracts.TimeType.Free), false, false, orderKey)
        {
            _Item = item;
        }

        #region data
        private ISlottedItem _Item;
        #endregion

        public HoldingSlot ItemSlot => Source as HoldingSlot;
        public override string Key => $@"HoldSecondSlot.{_Item?.ID ?? Guid.Empty}";
        public override string DisplayName(CoreActor actor)
        {
            var _info = GetInfoData.GetInfoFeedback(_Item, actor) as ObjectInfo;
            var _title = _info?.Message ?? $@"Held Object in {_Item?.MainSlot.ActionName}";
            return $@"Grab {_title} into {ItemSlot.ActionName}";
        }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Two-hand grab", activity.Actor, observer, ItemSlot.SlottedItem as CoreObject);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // slot to both primary and secondary
            _Item?.SetItemSlot(_Item?.MainSlot, ItemSlot);
            activity.EnqueueRegisterPreEmptively(Budget);

            // feedback
            var _info = _Item != null
                ? GetInfoData.GetInfoFeedback(_Item, ItemSlot.Creature)
                : null;
            if (_info != null)
            {
                var _step = activity.GetActivityResultNotifyStep(@"Hold in Two Hands");
                _step.SysNotify.Infos.Add(_info);
                _step.AppendFollowing(activity.GetNotifyStep(
                    new RefreshNotify(true, false, false, true, false)));
                return _step;
            }
            else
            {
                var _step = activity.GetActivityResultNotifyStep(@"Hold in Two Hands");
                _step.AppendFollowing(activity.GetNotifyStep(
                    new RefreshNotify(true, false, false, true, false)));
                return _step;
            }
        }
    }
}
