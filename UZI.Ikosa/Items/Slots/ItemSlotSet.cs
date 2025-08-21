using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class ItemSlotSet : IMonitorChange<CoreItem>, INotifyCollectionChanged
    {
        protected List<ItemSlot> _Slots;

        #region Construction
        public ItemSlotSet()
        {
            _Slots = [];
        }
        #endregion

        public void Add(ItemSlot slot)
        {
            _Slots.Add(slot);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, slot));
        }

        /// <summary>Does slot type exist?</summary>
        public bool Contains(string slotType)
            => (from _slot in _Slots
                where _slot.SlotType.Equals(slotType, StringComparison.OrdinalIgnoreCase)
                select _slot).Any();

        /// <summary>Does an empty slot of this type exist?</summary>
        public bool Contains(string slotType, bool empty)
            => empty
            ? (from _slot in _Slots
               where _slot.SlotType.Equals(slotType, StringComparison.OrdinalIgnoreCase)
               && (_slot.SlottedItem == null)
               select _slot).Any()
            : Contains(slotType);

        /// <summary>Does a slot of this type and sub-type exist?</summary>
        public bool Contains(string slotType, string subType)
            => _Slots.Any(_s => _s.SlotType == slotType && _s.SubType == subType);

        public ItemSlot this[string slotType]
            => _Slots.FirstOrDefault(_s => _s.SlotType == slotType);

        public ItemSlot this[string slotType, bool empty]
            => (empty)
            ? _Slots.FirstOrDefault(_s => _s.SlotType == slotType && _s.SlottedItem == null)
            : this[slotType];

        public ItemSlot this[string slotType, string subType]
            => _Slots.FirstOrDefault(_s => _s.SlotType == slotType && _s.SubType == subType);

        public ItemSlot this[string slotType, string subType, bool empty]
            => (empty)
            ? _Slots.FirstOrDefault(_s =>
                         (_s.SlotType == slotType)
                         && (_s.SubType == subType)
                         && (_s.SlottedItem == null))
            : this[slotType, subType];

        /// <summary>Returns all distinct held objects in any holding slot subtype</summary>
        public IEnumerable<ICoreObject> HeldObjects
            => (from _slot in AllSlots.OfType<HoldingSlot>()
                where (_slot.SlottedItem != null)
                select _slot.SlottedItem.BaseObject).Distinct();

        /// <summary>Returns all distinct held objects in off-hands</summary>
        public IEnumerable<ICoreObject> HeldObjectsOffHand
            => (from _slot in AllSlots.OfType<HoldingSlot>()
                where _slot.IsOffHand
                && (_slot.SlottedItem != null)
                select _slot.SlottedItem.BaseObject).Distinct();

        /// <summary>Gets the slot that contains an object</summary>
        public ItemSlot SlotForItem(ICoreObject coreObject)
            => (from _slot in AllSlots
                where (_slot.SlottedItem != null)
                where (_slot.SlottedItem.BaseObject == coreObject)
                select _slot).FirstOrDefault();

        #region public void Remove(ItemSlot slot)
        public void Remove(ItemSlot slot)
        {
            _Slots.Remove(slot);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion

        public HoldingSlot GetFreeHand(HoldingSlot except)
            => AllSlots
            .OfType<HoldingSlot>()
            .FirstOrDefault(_s => _s.SlottedItem == null && (_s != except));

        public int Count
            => _Slots.Count;

        public IEnumerable<ItemSlot> AllSlots
            => _Slots.Select(_s => _s);

        public IEnumerable<ItemSlot> AvailableSlots(ISlottedItem item)
            => _Slots
            .Where(_s => _s.SlotType == item.SlotType && _s.SlottedItem == null)
            .OrderBy(_s => _s.SlotType)
            .ThenBy(_s => _s.SubType);

        public readonly struct SlotIndexer
        {
            internal SlotIndexer(string type, string subType)
            {
                SlotType = type;
                SlotSubType = subType;
            }
            public readonly string SlotType;
            public readonly string SlotSubType;
        }

        #region IMonitorChange<CoreItem> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreItem> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreItem> args)
        {
            if (args.Action.Equals(@"Remove", StringComparison.OrdinalIgnoreCase))
            {
                var _slot = SlotForItem(args.NewValue);
                if (_slot != null)
                {
                    _slot.SlottedItem.ClearSlots();
                }
            }
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreItem> args)
        {
        }

        #endregion

        #region INotifyCollectionChanged Members
        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion
    }
}
