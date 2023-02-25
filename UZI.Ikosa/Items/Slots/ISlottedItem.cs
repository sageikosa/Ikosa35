using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Information about a slotted item.
    /// </summary>
    public interface ISlottedItem : IItemBase, IControlChange<SlotChange>, ILinkOwner<LinkableDock<ISlottedItem>>
    {
        string SlotType { get; }
        ItemSlot MainSlot { get; }
        ItemSlot SecondarySlot { get; }
        IEnumerable<ItemSlot> AllSlots { get; }
        void ClearSlots();
        void SetItemSlot(ItemSlot slot);
        void SetItemSlot(ItemSlot slotA, ItemSlot slotB);
        ICoreObject BaseObject { get; }
        ActionTime SlottingTime { get; }
        bool SlottingProvokes { get; }
        ActionTime UnslottingTime { get; }
        bool UnslottingProvokes { get; }

        /// <summary>True if the item can be transferred to another owner</summary>
        bool IsTransferrable { get; }
    }

    public enum SlotChange { Clear, Set }
}
