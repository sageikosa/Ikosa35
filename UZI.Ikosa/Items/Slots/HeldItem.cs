using System;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class HeldItem
    {
        internal HeldItem(ISlottedItem item, params string[] slotsHolding)
        {
            ItemID = item.ID;
            Slots = slotsHolding;
            MainHead = 0;
        }

        internal HeldItem(ISlottedItem item, int dominantHead, params string[] slotsHolding)
        {
            ItemID = item.ID;
            Slots = slotsHolding;
            MainHead = dominantHead;
        }

        public Guid ItemID { get; private set; }
        public string[] Slots { get; private set; }
        public int MainHead { get; private set; }
    }
}
