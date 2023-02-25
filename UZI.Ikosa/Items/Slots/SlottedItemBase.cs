using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class SlottedItemBase : ItemBase, ISlottedItem
    {
        #region construction
        protected SlottedItemBase(string name, string slotType)
            : base(name, Size.Tiny)
        {
            _SlotType = slotType;
            _MainSlot = new BiDiPtr<ItemSlot, ISlottedItem>(this);
            _SecondarySlot = new BiDiPtr<ItemSlot, ISlottedItem>(this);
            _SetCtrl = new ChangeController<SlotChange>(this, SlotChange.Clear);
        }
        #endregion

        #region private data
        protected string _SlotType;
        protected BiDiPtr<ItemSlot, ISlottedItem> _MainSlot;
        protected BiDiPtr<ItemSlot, ISlottedItem> _SecondarySlot;
        #endregion

        #region ISlottedItem Members
        public string SlotType => _SlotType;
        public ItemSlot MainSlot => _MainSlot.LinkDock;
        public ItemSlot SecondarySlot => _SecondarySlot.LinkDock;

        public IEnumerable<ItemSlot> AllSlots
        {
            get
            {
                if (_MainSlot.LinkDock != null)
                {
                    yield return _MainSlot.LinkDock;
                    if (_SecondarySlot.LinkDock != null)
                        yield return _SecondarySlot.LinkDock;
                }
                yield break;
            }
        }

        /// <summary>broadcasts change and adds slotted adjunct</summary>
        protected void DoSlotSet()
        {
            // action provider?
            if ((this is IActionProvider) && !CreaturePossessor.Actions.Providers.ContainsKey(this))
                CreaturePossessor.Actions.Providers.Add(this, (IActionProvider)this);

            // action filters
            if ((this is IActionFilter) && !CreaturePossessor.Actions.Filters.ContainsKey(this))
                CreaturePossessor.Actions.Filters.Add(this, (IActionFilter)this);

            _SetCtrl.DoValueChanged(SlotChange.Set);
            AddAdjunct(new Slotted(MainSlot));
            AddAdjunct(new Attended(CreaturePossessor));
        }

        /// <summary>removes slotted adjunct and broadcasts change</summary>
        protected void DoSlotCleared(ItemSlot slotA)
        {
            if ((this is IActionProvider) && CreaturePossessor.Actions.Providers.ContainsKey(this))
                CreaturePossessor.Actions.Providers.Remove(this);
            if ((this is IActionFilter) && CreaturePossessor.Actions.Filters.ContainsKey(this))
                CreaturePossessor.Actions.Filters.Remove(this);

            foreach (var _slotted in Adjuncts.OfType<Slotted>().Where(_s => _s.ItemSlot == slotA).ToList())
            {
                _slotted.Eject();
            }
            foreach (var _attended in Adjuncts.OfType<Attended>().ToList())
            {
                _attended.Eject();
            }
            _SetCtrl.DoValueChanged(SlotChange.Clear);
        }

        public abstract bool IsTransferrable { get; }

        #region IControlChange<bool> Members
        private ChangeController<SlotChange> _SetCtrl;
        public void AddChangeMonitor(IMonitorChange<SlotChange> subscriber)
        {
            _SetCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<SlotChange> subscriber)
        {
            _SetCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        protected virtual bool FinalSlotCheck(ItemSlot slot)
            => true;

        #region protected bool SlotCheck(ItemSlot slot)
        protected bool SlotCheck(ItemSlot slot)
        {
            if (slot != null)
            {
                if ((Possessor == slot.Creature) && (slot.SlotType.Equals(SlotType)))
                    return FinalSlotCheck(slot);
            }
            return false;
        }
        #endregion

        /// <summary>called just before item slots are cleared</summary>
        protected virtual void OnPreClearSlots() { }

        /// <summary>called just after item slots are cleared</summary>
        protected virtual void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            DoSlotCleared(slotA);
        }

        #region public void ClearSlots()
        public void ClearSlots()
        {
            if (!_MainSlot.WillAbortChange(null) && !_SecondarySlot.WillAbortChange(null))
            {
                OnPreClearSlots();
                var _main = _MainSlot.LinkDock;
                var _secondary = _SecondarySlot.LinkDock;

                _MainSlot.LinkDock = null;
                _SecondarySlot.LinkDock = null;

                // perform tear-down after actual clearage
                OnClearSlots(_main, _secondary);
                this.IncreaseSerialState();
            }
        }
        #endregion

        /// <summary>Called just before item slots are set</summary>
        protected virtual void OnPreSetItemSlot(ref ItemSlot slotA, ref ItemSlot slotB) { }

        /// <summary>called after item slots are set</summary>
        protected virtual void OnSetItemSlot()
        {
            DoSlotSet();
        }

        #region public void SetItemSlot(ItemSlot slot)
        public void SetItemSlot(ItemSlot slot)
        {
            if (SlotCheck(slot))
            {
                if (!_MainSlot.WillAbortChange(slot) && !_SecondarySlot.WillAbortChange(null))
                {
                    ItemSlot _null = null;
                    OnPreSetItemSlot(ref slot, ref _null);
                    _MainSlot.LinkDock = null;
                    _SecondarySlot.LinkDock = null;
                    _MainSlot.LinkDock = slot;
                    _SecondarySlot.LinkDock = _null;
                    this.IncreaseSerialState();
                }
            }
            else
            {
                throw new ArgumentException("Item-slot not valid for this item.");
            }
        }
        #endregion

        #region public void SetItemSlot(ItemSlot slotA, ItemSlot slotB)
        public void SetItemSlot(ItemSlot slotA, ItemSlot slotB)
        {
            if (SlotCheck(slotA) && SlotCheck(slotB) && (slotA != slotB))
            {
                if (!_MainSlot.WillAbortChange(slotA) && !_SecondarySlot.WillAbortChange(slotB))
                {
                    OnPreSetItemSlot(ref slotA, ref slotB);
                    _MainSlot.LinkDock = null;
                    _SecondarySlot.LinkDock = null;
                    _MainSlot.LinkDock = slotA;
                    _SecondarySlot.LinkDock = slotB;
                    this.IncreaseSerialState();
                }
            }
            else
            {
                throw new ArgumentException("Item-slot not valid for this item.");
            }
        }
        #endregion

        public virtual ICoreObject BaseObject
            => this;

        public abstract ActionTime SlottingTime { get; }
        public abstract ActionTime UnslottingTime { get; }
        public abstract bool SlottingProvokes { get; }
        public abstract bool UnslottingProvokes { get; }

        #region ILinkOwner<LinkableDock<ISlottedItem>> Members

        public void LinkDropped(LinkableDock<ISlottedItem> reference)
        {
            // confirm it is an itemSlot
            if (reference is ItemSlot _itemSlot)
            {
                // slot matches the reference, but it no longer points to this item...
                if ((_itemSlot == _MainSlot.LinkDock) && (_MainSlot.LinkDock.SlottedItem != this))
                {
                    // NOTE: clearing the main slot, will clear both
                    ClearSlots();
                }
                if ((_itemSlot == _SecondarySlot.LinkDock) && (_SecondarySlot.LinkDock.SlottedItem != this))
                {
                    _SecondarySlot.LinkDock = null;
                }
            }
        }

        public void LinkAdded(LinkableDock<ISlottedItem> changer)
        {
            OnSetItemSlot();
        }

        #endregion
    }
}
