using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class DivineFocusComponent : SpellComponent, IMonitorChange<ISlottedItem>
    {
        public override string ComponentName => @"Divine Focus";

        private ItemSlot _Slot = null;

        public override bool CanStartActivity(Creature caster)
            => caster.Body.ItemSlots.AllSlots
                .Any(_s => (_s.SlotType == ItemSlot.DevotionalSymbol)
                && (_s.SlottedItem as DevotionalSymbol)?.Devotion == caster.Devotion?.Name);

        public override void StartUse(CoreActivity activity)
        {
            var _caster = activity.Actor as Creature;
            _Slot = _caster.Body.ItemSlots.AllSlots
                .FirstOrDefault(_s => (_s.SlotType == ItemSlot.DevotionalSymbol)
                && (_s.SlottedItem as DevotionalSymbol)?.Devotion == _caster.Devotion?.Name);
            _Slot?.AddChangeMonitor(this);
        }

        public override void StopUse(CoreActivity activity)
        {
            _Slot?.RemoveChangeMonitor(this);
            _Slot = null;
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => CanStartActivity(activity.Actor as Creature);

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            var _caster = _Slot?.Creature;
            if (!((args.NewValue as DevotionalSymbol)?.Devotion == _caster?.Devotion?.Name))
            {
                HasFailed = true;
            }
        }
    }
}
