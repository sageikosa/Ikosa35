using System;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class OffHand : Adjunct
    {
        public OffHand(object source, ItemSlot slot)
            : base(source)
        {
            _Slot = slot;
        }

        public override bool IsProtected { get { return true; } }

        private ItemSlot _Slot;
        public ItemSlot Slot { get { return _Slot; } }

        public override object Clone()
        {
            return new OffHand(Source, Slot);
        }
    }
}
