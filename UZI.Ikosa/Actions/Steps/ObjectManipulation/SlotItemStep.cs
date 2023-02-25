using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SlotItemStep : CoreStep
    {
        public SlotItemStep(CoreActivity activity, HoldingWrapper wrapper, ItemSlot slot)
            : base(activity)
        {
            _Wrapper = wrapper;
            _Slot = slot;
        }

        #region private data
        private HoldingWrapper _Wrapper;
        private ItemSlot _Slot;
        #endregion

        public HoldingWrapper HoldingWrapper => _Wrapper;
        public ItemSlot ItemSlot => _Slot;

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            // get the thing being held
            if (HoldingWrapper.BaseObject is ISlottedItem _item)
            {
                // first, stop holding it...
                HoldingWrapper.ClearSlots();
                if (HoldingWrapper.MainSlot == null)
                {
                    // so far so good
                    _item.SetItemSlot(_Slot);
                }

                // make sure it ended up somewhere
                if (_item.MainSlot == null)
                {
                    // didn't get slotted, so drop it somewhere...
                    Drop.DoDrop(_item.CreaturePossessor, _item, this, true);
                }
            }
            // TODO: activity feedback...
            return true;
        }
    }
}
