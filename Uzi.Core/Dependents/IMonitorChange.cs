namespace Uzi.Core
{
    /// <summary>
    /// May veto a proposed change, and may be interested in changes after they occur.
    /// </summary>
    public interface IMonitorChange<LinkType>
    {
        void PreTestChange(object sender, AbortableChangeEventArgs<LinkType> args);
        void PreValueChanged(object sender, ChangeValueEventArgs<LinkType> args);
        void ValueChanged(object sender, ChangeValueEventArgs<LinkType> args);
    }
}
