namespace Uzi.Ikosa.Proxy.ViewModel
{
    public abstract class QueuedTargetItem
    {
        public QueuedTargetItem(ObservableActor actor)
        {
            ObservableActor = actor;
        }

        public ObservableActor ObservableActor { get; set; }
    }
}
