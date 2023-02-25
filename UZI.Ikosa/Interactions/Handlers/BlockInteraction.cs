using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    /// <summary>Unconditionally blocks an interaction type</summary>
    public class BlockInteraction<Interact> : IInteractHandler where Interact: InteractData
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Interact)
                workSet.Feedback.Add(new UnderstoodFeedback(this));
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Interact);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }
}
