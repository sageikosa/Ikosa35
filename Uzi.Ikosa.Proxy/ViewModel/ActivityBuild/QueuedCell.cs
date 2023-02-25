using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class QueuedCell : QueuedTargetItem
    {
        public QueuedCell(ObservableActor actor, ICellLocation location)
            : base(actor)
        {
            Location = location;
        }

        public ICellLocation Location { get; set; }
    }
}