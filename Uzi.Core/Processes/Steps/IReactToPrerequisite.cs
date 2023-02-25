namespace Uzi.Core
{
    /// <summary>Reactor that reacts to prerequisite dispensation.</summary>
    public interface IReactToPrerequisite : ICanReact
    {
        void ReactToPrerequisite(CoreStep triggerStep, StepPrerequisite triggerPrerequisite);
    }
}
