using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Items.Weapons;
using System.Linq;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class HeldItemsGroup : ICreatureBound
    {
        #region Construction (Snapshot)
        /// <summary>
        /// Creates the set with the items currently in the creature's holding item slots
        /// </summary>
        /// <param name="creature"></param>
        internal HeldItemsGroup(Creature creature)
        {
            _HeldItems = [];
            _MainItem = null;
            Creature = creature;

            // take a snapshot
            foreach (ItemSlot _slot in Creature.Body.ItemSlots.AllSlots)
            {
                // capture slotted items in holding slots (that we don't already know about)
                if (_slot.SlotType.Equals(ItemSlot.HoldingSlot) && (_slot.SlottedItem != null)
                    && !this.Contains(_slot.SlottedItem))
                {
                    ISlottedItem _sItem = _slot.SlottedItem;
                    if (_sItem.SecondarySlot != null)
                    {
                        // using a secondary slow, so see if its a double weapon
                        if (_sItem is DoubleMeleeWeaponBase)
                        {
                            _HeldItems.Add(new HeldItem(_sItem, ((DoubleMeleeWeaponBase)_sItem).MainHeadIndex, _sItem.MainSlot.SubType, _sItem.SecondarySlot.SubType));
                        }
                        else
                        {
                            _HeldItems.Add(new HeldItem(_sItem, _sItem.MainSlot.SubType, _sItem.SecondarySlot.SubType));
                        }
                    }
                    else
                    {
                        // not using a secondary slot, but still see if its a double weapon
                        if (_sItem is DoubleMeleeWeaponBase)
                        {
                            _HeldItems.Add(new HeldItem(_sItem, ((DoubleMeleeWeaponBase)_sItem).MainHeadIndex, _sItem.MainSlot.SubType));
                        }
                        else
                        {
                            _HeldItems.Add(new HeldItem(_sItem, _sItem.MainSlot.SubType));
                        }
                    }
                }
            }
        }
        #endregion

        // For each holding item, cross reference the slots in it
        private Collection<HeldItem> _HeldItems;

        #region public IEnumerable<HeldItem> GetHeldItems()
        /// <summary>
        /// Enumerate all held items in this group
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HeldItem> GetHeldItems()
        {
            foreach (HeldItem _item in _HeldItems)
            {
                yield return _item;
            }
            yield break;
        }
        #endregion

        #region public bool Contains(ISlottedItem item)
        /// <summary>
        /// Indicates whether the group contains the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ISlottedItem item)
        {
            return _HeldItems.Where(_item => _item.ItemID == item.ID).Count() > 0;
        }
        #endregion

        #region public bool SlotInUse(string slotSubType)
        /// <summary>
        /// Indicates whether an existing held item is using the specified slot in this group
        /// </summary>
        /// <param name="slotSubType"></param>
        /// <returns></returns>
        public bool SlotInUse(string slotSubType)
        {
            return _HeldItems.Where(_item => _item.Slots.Contains(slotSubType)).Count() > 0;
        }
        #endregion

        #region public IEnumerable<ISlottedItem> GetItems()
        /// <summary>
        /// Get all items in the held item group
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ISlottedItem> GetItems()
        {
            foreach (HeldItem _item in _HeldItems)
            {
                yield return (ISlottedItem)Creature.Possessions[_item.ItemID];
            }
            yield break;
        }
        #endregion

        #region public void SwitchTo()
        /// <summary>Switch to this held item set.</summary>
        /// <remarks>
        /// Not suitable for turn-based play, as it does not account for unwield and wield action times.  
        /// Intended only to generate summary sheet-style output.
        /// </remarks>
        public void SwitchTo()
        {
            if (IsValid)
            {
                foreach (HeldItem _held in _HeldItems)
                {
                    // get item
                    ISlottedItem _sItem = Creature.Possessions[_held.ItemID] as ISlottedItem;
                    if (_sItem != null)
                    {
                        // put it in the appropriate slot(s)
                        if (_held.Slots.Length > 1)
                        {
                            _sItem.SetItemSlot(Creature.Body.ItemSlots[ItemSlot.HoldingSlot, _held.Slots[0]],
                                Creature.Body.ItemSlots[ItemSlot.HoldingSlot, _held.Slots[1]]);
                        }
                        else
                        {
                            _sItem.SetItemSlot(Creature.Body.ItemSlots[ItemSlot.HoldingSlot, _held.Slots[0]]);
                        }

                        // set the dominant head (if appropriate)
                        DoubleMeleeWeaponBase _dbl = _sItem as DoubleMeleeWeaponBase;
                        if (_dbl != null)
                        {
                            _dbl.MainHeadIndex = _held.MainHead;
                        }
                    }
                }
            }
        }
        #endregion

        #region public bool IsValid { get; }
        /// <summary>
        /// If the items in the holding set are not in the creature's possessions, then false
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (HeldItem _held in _HeldItems)
                {
                    if (!Creature.Possessions.Contains(_held.ItemID))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion

        #region public HeldItem MainItem { get; set; }
        private HeldItem _MainItem;
        /// <summary>
        /// First item in a full attack action
        /// </summary>
        public HeldItem MainItem
        {
            get
            {
                return _MainItem;
            }
            set
            {
                if (_HeldItems.Contains(value))
                {
                    _MainItem = value;
                }
            }
        }
        #endregion

        public Creature Creature { get; private set; }
    }
}
