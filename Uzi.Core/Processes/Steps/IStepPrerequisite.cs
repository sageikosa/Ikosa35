namespace Uzi.Core
{
    public interface IStepPrerequisite
    {
        string BindKey { get; }
        bool FailsProcess { get; }
        CoreActor Fulfiller { get; }
        bool IsReady { get; }
        bool IsSerial { get; }
        string Name { get; }
        Qualifier Qualification { get; }
        object Source { get; }
        bool UniqueKey { get; }
    }
}
