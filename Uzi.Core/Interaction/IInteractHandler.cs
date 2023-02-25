using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>
    /// Generally stateless handler interface, to be implemented by helper classes
    /// </summary>
    public interface IInteractHandler
    {
        /// <summary>
        /// Stateless handler interface, to be implemented by helper classes
        /// </summary>
        /// <param name="target">object that needs handling</param>
        /// <param name="interact">the actual interaction</param>
        /// <param name="feedback">feedback about the interaction</param>
        /// <returns>true if event is consumed completely</returns>
        void HandleInteraction(Interaction workSet);

        /// <summary>
        /// List the types this handler interacts with.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetInteractionTypes();

        /// <summary>
        /// Determine whether the handler should be placed in the handler chain before the specified handler.
        /// </summary>
        /// <remarks>This is called once per existing handler in a handler chain until the call returns true.</remarks>
        /// <param name="existingHandler"></param>
        /// <returns></returns>
        bool LinkBefore(Type interactType, IInteractHandler existingHandler);
    }
}
