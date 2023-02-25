namespace Uzi.Core
{
    /// <summary>Something that reacts to CoreProcess activity</summary>
    public interface ICanReact
    {
        bool IsFunctional { get; }
    }

    /// <summary>Something that may unconditionally suppress CoreProcess starts (and thus prevent side-effects and new process spawns)</summary>
    public interface ICanReactBySuppress : ICanReact
    {
        void ReactToProcessBySuppress(CoreProcess process);
    }

    /// <summary>Something that reacts to CoreProcess starts (typically not by suppression)</summary>
    public interface ICanReactBySideEffect : ICanReact
    {
        void ReactToProcessBySideEffect(CoreProcess process);
    }

    /// <summary>
    /// Channel to generate a new process on response to a process starting
    /// </summary>
    public interface ICanReactWithNewProcess : ICanReact
    {
        void ReactToProcessByStep(CoreProcess process);
    }

    public interface ICanReactToStepComplete : ICanReact
    {
        /// <summary>Returns true if the reactor could react to the step</summary>
        bool CanReactToStepComplete(CoreStep step);

        /// <summary>React to the step</summary>
        void ReactToStepComplete(CoreStep step);
    }
}
