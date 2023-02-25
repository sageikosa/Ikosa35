namespace Uzi.Core
{
    /// <summary>
    /// Indicates the handler may process feedback that has been processed from farther down the interaction chain.
    /// </summary>
    public interface IProcessFeedback : IInteractHandler
    {
        /// <summary>
        /// look at feedback from a handled interaction and alter if necessary
        /// </summary>
        void ProcessFeedback(Interaction workSet);
    }
}
