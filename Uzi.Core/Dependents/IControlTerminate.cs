namespace Uzi.Core
{
    public interface IControlTerminate
    {
        void DoTerminate();
        void AddTerminateDependent(IDependOnTerminate subscriber);
        void RemoveTerminateDependent(IDependOnTerminate subscriber);
        int TerminateSubscriberCount { get; }
    }
}
