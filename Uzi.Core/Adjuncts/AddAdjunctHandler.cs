using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class AddAdjunctHandler : IInteractHandler
    {
        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _data = workSet.InteractData as AddAdjunctData;
                if (_data != null)
                {
                    var _anchor = workSet.Target as IAdjunctable;
                    if (_anchor != null)
                    {
                        // try to add
                        var _added = _anchor.Adjuncts.Add(_data.Adjunct);
                        workSet.Feedback.Add(new ValueFeedback<bool>(this, _added));
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
            yield return typeof(AddAdjunctData);
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
