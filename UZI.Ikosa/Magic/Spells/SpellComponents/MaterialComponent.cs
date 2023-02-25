using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class MaterialComponent : SpellComponent, IMonitorChange<ISlottedItem>
    {
        public override string ComponentName => @"Material";

        private Creature _Critter = null;
        private List<HoldingSlot> _HoldSlots = null;
        private List<ItemSlot> _Pouches = null;

        /// <summary>holding a component pouch, or has a free hand and a component pouch properly slotted</summary>
        private bool IsValid(Creature caster)
            => caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().Any(_h => _h.SlottedItem is ComponentPouch)
            || (caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().Any(_h => (_h.SlottedItem == null))
                && caster.Body.ItemSlots.AllSlots.Any(_s => _s.SlotType == ItemSlot.Pouch && _s.SlottedItem is ComponentPouch));

        public override bool CanStartActivity(Creature caster)
            => IsValid(caster);

        public override void StartUse(CoreActivity activity)
        {
            if (activity.Actor is Creature _critter)
            {
                // ensure we don't forget the creature
                _Critter = _critter;

                // track holding slots
                _HoldSlots = _critter.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().ToList();
                foreach (var _h in _HoldSlots)
                {
                    _h.AddChangeMonitor(this);
                }

                // track pouches
                _Pouches = _critter.Body.ItemSlots.AllSlots.Where(_s => _s.SlotType == ItemSlot.Pouch).ToList();
                foreach (var _p in _Pouches)
                {
                    _p.AddChangeMonitor(this);
                }
            }
        }

        public override void StopUse(CoreActivity activity)
        {
            // stop tracking pouches
            if (_Pouches != null)
            {
                foreach (var _p in _Pouches)
                {
                    _p.RemoveChangeMonitor(this);
                }
            }
            _Pouches = null;

            // stop tracking holding slots
            if (_HoldSlots != null)
            {
                foreach (var _h in _HoldSlots)
                {
                    _h.RemoveChangeMonitor(this);
                }
            }
            _HoldSlots = null;
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => CanStartActivity(activity.Actor as Creature);

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            if (!IsValid(_Critter))
            {
                HasFailed = true;
            }
        }
    }
}
