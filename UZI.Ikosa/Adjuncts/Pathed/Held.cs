using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Applied to any object held in a holding slot</summary>
    [Serializable]
    public class Held : Pathed, ISlotPathed
    {
        /// <summary>Applied to any object held in a holding slot</summary>
        public Held(HoldingWrapper wrapper)
            : base(wrapper)
        {
        }

        public HoldingWrapper HoldingWrapper
            => Source as HoldingWrapper;

        /// <summary>Held is controlled by being placed in a holding wrapper.</summary>
        public override bool IsProtected
            => true;

        public override object Clone()
            => new Held(HoldingWrapper);

        public override IAdjunctable GetPathParent()
            => HoldingWrapper;

        public override string GetPathPartString()
            => @".held";

        // ISlotPathed

        public IEnumerable<ItemSlot> PathedSlots
            => HoldingWrapper.AllSlots;

        public ISlottedItem SlottedConnector
            => HoldingWrapper;

        public override void UnPath()
        {
            HoldingWrapper.ClearSlots();
        }
    }
}