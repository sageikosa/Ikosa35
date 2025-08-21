using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class FindAdjunctDataExcluderHandler : IInteractHandler
    {
        /// <summary>Implicitly Any in the list of finds</summary>
        public FindAdjunctDataExcluderHandler()
        {
            _Exclude = [];
        }

        private List<Type> _Exclude;

        public List<Type> Exclusions => _Exclude;

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is FindAdjunctData _find)
            {
                // if this is filtering, then block by reporting the request was understood as the only feedback
                // since any feedback prevents the handler chain from progressing, understood is a good stopgap
                if (Exclusions.Any(_e => _find.AdjunctTypes.Contains(_e)))
                {
                    workSet.Feedback.Add(new UnderstoodFeedback(this));
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
            return true;
        }
        #endregion
    }
}
