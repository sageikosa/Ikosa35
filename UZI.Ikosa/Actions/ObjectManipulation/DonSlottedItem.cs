using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Note: not used to put items into holding slot</summary>
    [Serializable]
    public class DonSlottedItem : ActionBase
    {
        /// <summary>Note: not used to put items into holding slot</summary>
        public DonSlottedItem(HoldingSlot holdSlot, HoldingWrapper wrapper, ItemSlot itemSlot, string orderKey)
            : base(holdSlot, (wrapper.BaseObject as ISlottedItem).SlottingTime,
                  (wrapper.BaseObject as ISlottedItem).SlottingProvokes, false, orderKey)
        {
            _Slot = itemSlot;
            _Wrapper = wrapper;
        }

        #region data
        private HoldingWrapper _Wrapper;
        private ItemSlot _Slot;
        #endregion

        public HoldingSlot HoldingSlot => Source as HoldingSlot;
        public HoldingWrapper HoldingWrapper => _Wrapper;
        public ISlottedItem SlottedItem => HoldingWrapper.BaseObject as ISlottedItem;
        public ItemSlot ItemSlot => _Slot;

        public override string DisplayName(CoreActor actor)
            => $@"Put {SlottedItem.GetKnownName(actor)} on {_Slot.ActionName}";

        public override string Key
            => $@"DonItem.{ItemSlot.ID}";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Putting on Item", activity.Actor, observer, SlottedItem);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            return new SlotItemStep(activity, HoldingWrapper, _Slot);
        }
    }
}
