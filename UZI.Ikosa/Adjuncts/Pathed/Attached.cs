using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Attached : Pathed, ISlotPathed
    {
        public Attached(AttachmentWrapper wrapper)
            : base(wrapper)
        {
        }

        public AttachmentWrapper AttachmentWrapper
            => Source as AttachmentWrapper;

        /// <summary>Attached is controlled by being placed in an attachment wrapper.</summary>
        public override bool IsProtected
            => true;

        public override object Clone()
            => new Attached(AttachmentWrapper);

        public override IAdjunctable GetPathParent()
            => AttachmentWrapper;

        public override string GetPathPartString() => @".";

        public override void UnPath()
        {
            AttachmentWrapper.ClearSlots();
        }

        #region ISlotPathed Members

        public IEnumerable<ItemSlot> PathedSlots
        {
            get { return AttachmentWrapper.AllSlots; }
        }

        public ISlottedItem SlottedConnector
        {
            get { return AttachmentWrapper; }
        }

        #endregion
    }
}
