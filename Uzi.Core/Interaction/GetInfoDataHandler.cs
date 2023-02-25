using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class GetInfoDataHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is GetInfoData)
            {
                if (workSet.Target is ICoreObject _core)
                {
                    workSet.Feedback.Add(new InfoFeedback(this, _core.GetInfo(workSet.Actor, true)));
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetInfoData);
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
