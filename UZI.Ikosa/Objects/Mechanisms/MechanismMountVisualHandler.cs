using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MechanismMountVisualHandler : IProcessFeedback
    {
        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet?.Target is MechanismMount _mechMount)
            {
                if (workSet.InteractData is VisualPresentationData _vmData)
                {
                    foreach (var _vmdBack in workSet.Feedback.OfType<VisualModelFeedback>())
                    {
                        // NOTE: model is expected to fill a 5' cube
                        _vmdBack.ModelPresentation.BaseFace = _mechMount.MountFace;
                        _vmdBack.ModelPresentation.CubeFitScale = new Vector3D(_mechMount.Width / 5, _mechMount.Length / 5, _mechMount.Height / 5);
                        _vmdBack.ModelPresentation.IntraModelOffset = new Vector3D(_mechMount.XOffset, _mechMount.YOffset, _mechMount.ZOffset);
                        _vmdBack.ModelPresentation.Pivot = (_mechMount.Pivot % 8) * 45;
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }

        #endregion
    }
}
