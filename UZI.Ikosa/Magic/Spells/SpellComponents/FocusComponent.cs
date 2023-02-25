using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class FocusComponent : SpellComponent, IMonitorChange<ISlottedItem>
    {
        // TODO: focus item (type and/or cost)

        public FocusComponent(Type itemType, decimal cost)
        {
            _Type = itemType;
            _Cost = cost;
        }

        /// <summary>Inexpensive focus (assumed to be in component pouch)</summary>
        public FocusComponent()
        {
            _Type = null;
            _Cost = null;
        }

        public override string ComponentName => @"Focus";

        private List<HoldingSlot> _HoldSlots = null;
        private ItemSlot _Slot = null;
        private ISlottedItem _Item = null;

        #region data
        private Type _Type;
        private decimal? _Cost;
        #endregion

        public Type ItemType => _Type;
        public decimal? Cost => _Cost;

        /// <summary>holding a component pouch, or has a free hand and a component pouch properly slotted</summary>
        private bool IsValid(Creature caster)
            => (ItemType == null)
            ? caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().Any(_h => _h.SlottedItem == null)
                && caster.Body.ItemSlots.AllSlots.Any(_s => _s.SlotType == ItemSlot.Pouch && _s.SlottedItem is ComponentPouch)
            : caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .Any(_h => ItemType.IsAssignableFrom(_h.SlottedItem.BaseObject.GetType()));

        public override bool CanStartActivity(Creature caster)
            => IsValid(caster);

        public override void StartUse(CoreActivity activity)
        {
            if (activity.Actor is Creature _caster)
            {
                if (ItemType == null)
                {
                    _Slot = _caster.Body.ItemSlots.AllSlots.FirstOrDefault(_s => _s.SlotType == ItemSlot.Pouch && _s.SlottedItem is ComponentPouch);
                    _HoldSlots = _caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().ToList();
                    foreach (var _h in _HoldSlots)
                    {
                        _h.AddChangeMonitor(this);
                    }
                }
                else
                {
                    _Slot = _caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                        .FirstOrDefault(_h => ItemType.IsAssignableFrom(_h.SlottedItem.BaseObject.GetType()));
                }

                _Item = _Slot.SlottedItem;
                _Slot.AddChangeMonitor(this);
            }
        }

        public override void StopUse(CoreActivity activity)
        {
            _Slot?.RemoveChangeMonitor(this);
            _Slot = null;

            foreach (var _h in _HoldSlots)
            {
                _h.RemoveChangeMonitor(this);
            }
            _HoldSlots = null;
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            if (!IsValid(_Slot.Creature))
            {
                HasFailed = true;
            }
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => CanStartActivity(activity.Actor as Creature);
    }

    /// <summary>Specific costly focus items.</summary>
    /// <typeparam name="SDef">SpellDef associated with the focus item</typeparam>
    [Serializable]
    public class FocusItem<SDef> : ItemBase
        where SDef : SpellDef
    {
        public FocusItem(string name, Size naturalSize)
            : base(name, naturalSize)
        {
            // TODO: more...when needed
        }
    }
}
