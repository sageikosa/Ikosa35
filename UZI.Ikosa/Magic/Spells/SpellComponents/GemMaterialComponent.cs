using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Wealth;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class GemMaterialComponent<GM> : SpellComponent, IMonitorChange<ISlottedItem>
        where GM : GemMaterial
    {
        public GemMaterialComponent(decimal cost)
        {
            _Cost = cost;
        }

        #region private data
        private decimal _Cost;
        #endregion

        public override string ComponentName => @"Gem";
        public decimal Cost => _Cost;

        private HoldingSlot _Slot = null;
        private Gem _Gem = null;

        public override bool CanStartActivity(Creature caster)
            => caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .Any(_h => (_h.SlottedItem.BaseObject as Gem)?.ItemMaterial is GM);

        public override void StartUse(CoreActivity activity)
        {
            // TODO: build observable info...
            if (activity.Actor is Creature _critter)
            {

                (_Gem, _Slot) = (from _hold in _critter.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                                 let _g = (_hold.SlottedItem.BaseObject as Gem)
                                 where (_g != null) && (_g.ItemMaterial is GM) && (_g.Price.BasePrice >= Cost)
                                 select (gem: _g, slot: _hold)).FirstOrDefault();
                _Slot?.AddChangeMonitor(this);
            }
        }

        public override void StopUse(CoreActivity activity)
        {
            _Slot?.RemoveChangeMonitor(this);
            if (!HasFailed && (_Gem != null))
            {
                // destroy the item
                _Gem.StructurePoints = 0;
            }
            _Slot = null;
            _Gem = null;
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => (from _hold in ((activity.Actor as Creature)?.Body.ItemSlots.AllSlots.OfType<HoldingSlot>() ?? new HoldingSlot[] { })
                let _gem = (_hold.SlottedItem.BaseObject as Gem)
                where (_gem != null) && (_gem.ItemMaterial is GM) && (_gem.Price.BasePrice >= Cost)
                select _gem).Any();

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            if (_Slot?.SlottedItem != _Gem)
            {
                HasFailed = true;
            }
        }
    }
}
