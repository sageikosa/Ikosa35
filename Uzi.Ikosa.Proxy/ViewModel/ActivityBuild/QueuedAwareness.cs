using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class QueuedAwareness : QueuedTargetItem
    {
        public QueuedAwareness(ObservableActor actor, Guid id)
            : base(actor)
        {
            ID = id;
        }

        public Guid ID { get; set; }
        public AwarenessInfo Awareness
            => ObservableActor.SelectedAwarenesses.FirstOrDefault(_sa => _sa.ID == ID);

        public List<MenuBaseViewModel> ContextMenu
            => ActionMenuBuilder.GetContextMenu(ObservableActor.Actor, Awareness.SelectedItems.ToList());
    }

}