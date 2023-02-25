using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Applied to any item wield mounted</summary>
    [Serializable]
    public class WieldMounted : Pathed, ISlotPathed
    {
        /// <summary>Applied to any item wield mounted</summary>
        public WieldMounted(MountSlot mountSlot)
            : base(mountSlot)
        {
        }

        public MountSlot MountSlot { get { return Source as MountSlot; } }

        public override bool IsProtected { get { return true; } }

        public override IAdjunctable GetPathParent() { return MountSlot.SlottedItem; }

        public override string GetPathPartString() => @".";

        public override object Clone()
        {
            return new WieldMounted(MountSlot);
        }

        public static bool IsWieldMounted(IAdjunctable adjunctable)
            => adjunctable.HasAdjunct<WieldMounted>();

        public override void UnPath()
        {
            MountSlot.MountWrapper.UnmountItem();
        }

        #region ISlotPathed Members

        public IEnumerable<ItemSlot> PathedSlots
        {
            get
            {
                yield return MountSlot;
                yield break;
            }
        }

        public ISlottedItem SlottedConnector
        {
            get { return MountSlot.SlottedItem; }
        }

        #endregion
    }
}
