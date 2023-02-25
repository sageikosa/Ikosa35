using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class AttachmentSlot : ItemSlot, IActionProvider
    {
        public AttachmentSlot(AttachmentWrapper target, bool allowUnslotAction)
            : base(target, ItemSlot.Attachment, false)
        {
            _UnslotAct = allowUnslotAction;
        }

        #region state
        private bool _UnslotAct;
        #endregion

        public AttachmentWrapper AttachmentWrapper => Source as AttachmentWrapper;

        // IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if ((AttachmentWrapper?.DetachTime != null) && (SlottedItem != null))
            {
                yield return new DetachItem(this, AttachmentWrapper.DetachTime, @"100");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToItemSlotInfo();


        public override ItemSlot Clone(object source)
            => new AttachmentSlot(AttachmentWrapper, AllowUnSlotAction);
    }
}
