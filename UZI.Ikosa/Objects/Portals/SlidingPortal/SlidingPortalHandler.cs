using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{

    [Serializable]
    public class SlidingPortalHandler : IProcessFeedback
    {
        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet.Target is SlidingPortal _portal)
            {
                if (workSet.InteractData is VisualEffectData)
                {
                    // get related effects
                    var _veData = workSet.InteractData as VisualEffectData;
                    foreach (var _vedBack in workSet.Feedback.OfType<VisualEffectFeedback>()
                        .Where(_f => _f.VisualEffects.ContainsKey(typeof(SenseEffectExtension))))
                    {
                        foreach (var _new in _portal.VisualEffects(_veData.SourceRegion, _veData.Senses, _vedBack.VisualEffects[typeof(SenseEffectExtension)]))
                        {
                            _vedBack.VisualEffects[_new.Key] = _new.Value;
                        }
                    }
                }
                else if (workSet.InteractData is VisualPresentationData)
                {
                    var _vmData = workSet.InteractData as VisualPresentationData;
                    foreach (var _vmdBack in workSet.Feedback.OfType<VisualModelFeedback>())
                    {
                        var _obj = _portal.PortalledObjectA;
                        _vmdBack.ModelPresentation.CubeFitScale = new Vector3D(_obj.Width / 5, _obj.Thickness / 5, _obj.Height / 5);
                        _vmdBack.ModelPresentation.Pivot = _portal.OverridePivot(_vmdBack.ModelPresentation.Pivot);
                        _vmdBack.ModelPresentation.Tilt = _portal.OverrideTilt(_vmdBack.ModelPresentation.Tilt);
                        _vmdBack.ModelPresentation.BaseFace = AnchorFace.ZLow;
                        _vmdBack.ModelPresentation.CustomTransformInfos =
                            _portal.GetCustomTransformations(_vmData.TargetLocator.GeometricRegion);
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
            yield return typeof(VisualEffectData);
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
