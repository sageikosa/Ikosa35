using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.UI
{
    public class PresentableItemSlotVM : PresentableContext
    {
        public override ICoreObject CoreObject => ItemSlot?.SlottedItem;

        public ItemSlot ItemSlot { get; set; }

        public PresentableContext SlottedItem
            => ItemSlot.SlottedItem.GetPresentableObjectVM(VisualResources, Possessor);
    }
}
