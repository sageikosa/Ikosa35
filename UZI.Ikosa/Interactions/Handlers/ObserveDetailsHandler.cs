using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ObserveDetailsHandler : IInteractHandler
    {
        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObserveDetails);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            workSet.Feedback.Add(new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[] {
                new KeyValuePair<Guid, AwarenessLevel>(workSet.Target.ID, AwarenessLevel.Aware)
            }));
        }
        #endregion
    }
}
