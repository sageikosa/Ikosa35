using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Applied to any item that has been placed in an item slot</summary>
    [Serializable]
    public class Slotted : Pathed, ISlotPathed
    {
        /// <summary>Applied to any item that has been placed in an item slot</summary>
        public Slotted(ItemSlot slot)
            : base(slot)
        {
        }

        public ItemSlot ItemSlot 
            => Source as ItemSlot;

        /// <summary>Slotted is controlled by being placed in an item slot.</summary>
        public override bool IsProtected 
            => true;

        public override object Clone()
            => new Slotted(ItemSlot);

        public override IAdjunctable GetPathParent()
            => ItemSlot.Creature;

        public override string GetPathPartString()
        {
            var _slot = ItemSlot.SlotType switch
            {
                ItemSlot.WieldMount => @"Hip-Mount",
                ItemSlot.LargeWieldMount => @"Shoulder-Mount",
                ItemSlot.BackShieldMount => @"Back-Mount",
                ItemSlot.AmmoSash => @"Bandolier",
                ItemSlot.DevotionalSymbol => @"Symbol",
                _ => ItemSlot.SlotType
            };
            return (!string.IsNullOrEmpty(ItemSlot.SubType))
                ? $@"{_slot}/{ItemSlot.SubType}"
                : _slot;
        }

        public override void UnPath()
        {
            SlottedConnector.ClearSlots();
        }

        #region ISlotPathed Members

        public ItemSlot PathSlot
            => ItemSlot;

        public IEnumerable<ItemSlot> PathedSlots
            => (ItemSlot.SlottedItem != null)
            ? ItemSlot.SlottedItem.AllSlots
            : new ItemSlot[] { };

        public ISlottedItem SlottedConnector
            => ItemSlot.SlottedItem;

        #endregion
    }
}
