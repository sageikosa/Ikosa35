using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class UnslotItemStep : CoreStep
    {
        public UnslotItemStep(CoreActivity activity, SlottedItemBase slottedItem)
            : base(activity)
        {
            _SlottedItem = slottedItem;
        }

        private SlottedItemBase _SlottedItem;

        public SlottedItemBase SlottedItem => _SlottedItem;

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            // holdable
            var _holdable = HoldingSlot.GetHoldableItem(SlottedItem, SlottedItem.CreaturePossessor);

            // unslot item
            SlottedItem.ClearSlots();

            var _slot = SlottedItem.CreaturePossessor.Body.ItemSlots[ItemSlot.HoldingSlot, true];
            if (_slot != null)
            {
                if (SlottedItem.MainSlot == null)
                {
                    // and hold onto it
                    _holdable.SetItemSlot(_slot);
                    if (_holdable.MainSlot == null)
                    {
                        // didn't get held, so drop it
                        Drop.DoDrop(SlottedItem.CreaturePossessor, SlottedItem, this, true);
                    }
                }
            }
            else
            {
                // don't have a holding slot, so drop it
                Drop.DoDrop(SlottedItem.CreaturePossessor, SlottedItem, this, true);
            }

            // TODO: activity feedback...
            return true;
        }
    }
}
