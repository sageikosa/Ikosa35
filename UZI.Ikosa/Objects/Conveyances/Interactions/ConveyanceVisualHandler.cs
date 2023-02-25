using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceVisualHandler : IProcessFeedback
    {
        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet.Target is Conveyance _convey)
            {
                if (workSet.InteractData is VisualEffectData _veData)
                {
                    // get related effects
                    foreach (var _vedBack in workSet.Feedback.OfType<VisualEffectFeedback>()
                        .Where(_f => _f.VisualEffects.ContainsKey(typeof(SenseEffectExtension))))
                    {
                        foreach (var _new in _convey.Orientation
                            .VisualEffects(_veData.SourceRegion, _veData.Senses, _vedBack.VisualEffects[typeof(SenseEffectExtension)]))
                        {
                            _vedBack.VisualEffects[_new.Key] = _new.Value;
                        }
                    }
                }
                else if (workSet.InteractData is VisualPresentationData _vpData)
                {
                    foreach (var _vmdBack in workSet.Feedback.OfType<VisualModelFeedback>())
                    {
                        // NOTE: model is expected to fill a 5' cube
                        _vmdBack.ModelPresentation.CubeFitScale = new Vector3D(_convey.Width / 5, _convey.Length / 5, _convey.Height / 5);
                        _vmdBack.ModelPresentation.Tilt = (int)_convey.Orientation.Upright * 45d;
                        _vmdBack.ModelPresentation.TiltAxis = new Vector3D(0, 1, 0);
                        _vmdBack.ModelPresentation.IsFullOrigin = true;
                        _vmdBack.ModelPresentation.Twist = (_convey.Orientation.Twist + 1) * 90d;
                        _vmdBack.ModelPresentation.IntraModelOffset = _convey.Orientation.Displacement;

                        _vmdBack.ModelPresentation.Pivot = _convey.Orientation.GetModelPivot();
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
            return !(existingHandler is OverrideModelKey);
        }

        #endregion
    }
}
