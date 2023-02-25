using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class RemoveAdjunctHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                if (workSet.InteractData is RemoveAdjunctData _data)
                {
                    if (workSet.Target is IAdjunctable _anchor)
                    {
                        // try to remove
                        var _removed = _anchor.Adjuncts.Remove(_data.Adjunct);
                        workSet.Feedback.Add(new ValueFeedback<bool>(this, _removed));
                        return;
                    }
                }
            }

            // could not add
            workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
            return;
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(RemoveAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last (default) handler
            return false;
        }

        #endregion
    }
}
