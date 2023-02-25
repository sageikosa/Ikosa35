using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class CostlyMaterialComponent : SpellComponent, IMonitorChange<ISlottedItem>
    {
        public CostlyMaterialComponent(Type itemType, decimal cost)
        {
            _Type = itemType;
            _Cost = cost;
        }

        #region data
        private Type _Type;
        private decimal _Cost;
        #endregion

        public Type ItemType => _Type;
        public decimal Cost => _Cost;

        private HoldingSlot _Slot = null;
        private ISlottedItem _Item = null;

        public override string ComponentName => @"Costly Component";

        public override bool CanStartActivity(Creature caster)
            => caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .Any(_h => ItemType.IsAssignableFrom(_h.SlottedItem.BaseObject.GetType()));

        public override void StartUse(CoreActivity activity)
        {
            var _caster = activity.Actor as Creature;
            _Slot = _caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .FirstOrDefault(_h => ItemType.IsAssignableFrom(_h.SlottedItem.BaseObject.GetType()));
            _Item = _Slot.SlottedItem;
            _Slot?.AddChangeMonitor(this);
        }

        public override void StopUse(CoreActivity activity)
        {
            _Slot?.RemoveChangeMonitor(this);
            if (!HasFailed && (_Item !=null))
            {
                // destroy the item
                _Item.StructurePoints = 0;
            }
            _Slot = null;
            _Item = null;
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => (activity.Actor as Creature)?.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .Any(_h => ItemType.IsAssignableFrom(_h.SlottedItem.BaseObject.GetType())
                && _h.SlottedItem.Price.BasePrice >= Cost) ?? false;

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            if (_Slot?.SlottedItem != _Item)
            {
                HasFailed = true;
            }
        }
    }

    /// <summary>Specific costly material items.</summary>
    /// <typeparam name="SDef">SpellDef associated with the costly material item</typeparam>
    [Serializable]
    public class CostlyComponent<SDef> : ItemBase
        where SDef : SpellDef
    {
        public CostlyComponent(string name, Size naturalSize)
            : base(name, naturalSize)
        {
            // TODO: more...when needed
        }
    }
}
