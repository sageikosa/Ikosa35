using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class PresentableCreatureVM : PresentableThingVM<Creature>
    {
        public override PresentableCreatureVM Possessor { get => this; set { } }

        public void DoChangedPossessions()
            => DoPropertyChanged(nameof(ContextualPossessions));

        public void DoChangedItemSlots()
            => DoPropertyChanged(nameof(ContextualItemSlots));

        public IEnumerable<PresentableContext> ContextualPossessions
            => (from _p in Thing?.Possessions.All
                select _p.GetPresentableObjectVM(VisualResources, this)).ToList();

        public IEnumerable<PresentableItemSlotVM> ContextualItemSlots
            => (from _is in Thing?.Body.ItemSlots.AllSlots
                select new PresentableItemSlotVM
                {
                    ItemSlot = _is,
                    VisualResources = VisualResources
                }).ToList();

        public BitmapImagePart Portrait
            => Thing?.GetPortrait(VisualResources);

        public IEnumerable<BitmapImagePartListItem> AvailablePortraits
            => VisualResources.ResolvableImages;
    }
}
