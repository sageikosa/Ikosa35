using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class FindAdjunctDataHandler : IInteractHandler
    {
        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is FindAdjunctData)
            {
                var _find = workSet.InteractData as FindAdjunctData;
                if (workSet.Target is ICoreObject _target)
                {
                    // all adjuncts on target
                    workSet.Feedback.Add(new FindAdjunctFeedback(this,
                        _target.Adjuncts.Where(_a => _find.AdjunctTypes.Any(_at => _at.IsAssignableFrom(_a.GetType()))).ToList()));

                    foreach (var _conn in _target.Connected)
                    {
                        // and all reported from handling the interaction on the directly connected objects
                        workSet.Feedback.Add(new FindAdjunctFeedback(this, FindAdjunctData.FindAdjuncts(_conn, _find.AdjunctTypes).ToList()));
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(FindAdjunctData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion
    }
}
