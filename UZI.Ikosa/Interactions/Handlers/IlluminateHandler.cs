using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Things that shed light enable this handler to automagically illuminate objects as needed.  
    /// When requested, the handler's anchor tells the lightTarget how much light it would get 
    /// from this illuminator.
    /// </summary>
    [Serializable]
    public class IlluminateHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            var _isGeom = workSet.Actor == null;
            if (workSet.InteractData is Illuminate _illuminate)
            {
                // this is where the light should fall
                var _targetGeometry = _illuminate.LightTarget.GeometricRegion;

                // this is the "best" light info provided
                var _illuminator = _illuminate.Illuminator;

                // this is the geometry of the light handler
                var _lightGeom = _illuminator.SourceGeometry(_targetGeometry);

                var _lightPoint = _illuminator.InteractionPoint3D(_targetGeometry);
                var _nearest = (from _loc in _targetGeometry.AllCellLocations()
                                let _dist = IGeometricHelper.Distance(_lightPoint, _loc)
                                orderby _dist
                                select new { Location = _loc, Distance = _dist }).First();

                // get the light from the source (only works for material illuminators...)
                if (_illuminate?.Illuminator.PlanarPresence.HasMaterialPresence() ?? false)
                {
                    #region Get Lighting Level at Target Distance
                    // use best active light
                    LightRange _level;
                    if (_illuminator.FarShadowyLeft(_nearest.Location) < _nearest.Distance)
                        return;                    // distance is too great to be lit
                    else
                    {
                        var _shadowyLeft = _illuminator.ShadowyLeft(_nearest.Location);
                        if ((_shadowyLeft <= 0) || (_shadowyLeft < _nearest.Distance))
                            _level = LightRange.FarShadow;  // far shadowy
                        else
                        {
                            var _brightLeft = _illuminator.BrightLeft(_nearest.Location);
                            if ((_brightLeft <= 0) || (_brightLeft < _nearest.Distance))
                                _level = LightRange.NearShadow; // simply shadowy
                            else
                            {
                                var _veryLeft = _illuminator.VeryBrightLeft(_nearest.Location);
                                if ((_veryLeft <= 0) || (_veryLeft < _nearest.Distance))
                                    _level = LightRange.Bright;
                                else
                                    _level = LightRange.VeryBright;
                            }
                        }
                    }
                    #endregion

                    // prepare to track light to the target
                    long _maxLines = _targetGeometry.AllCorners().Count();
                    long _lineCount = 0;

                    // get shadow model
                    var _shadowModel = /*_isGeom ||*/ _illuminate.LightTarget.ShadowModel;

                    // NOTE: we are not carrying an interaction to check these...
                    var _filter = _illuminate.LightTarget.GeometricRegion.ContainingCube(_lightGeom);
                    foreach (var _lSet in _illuminate.LightTarget.EffectLinesFromSource(_lightPoint,
                        _targetGeometry, ITacticalInquiryHelper.EmptyArray, PlanarPresence.Material))
                    {
                        // 1: in shallow shadows, any target line yields the specified level
                        _lineCount++;
                        if (_shadowModel == ShadowModel.Normal)
                        {
                            workSet.Feedback.Add(new IlluminateResult(_illuminator, _level, _illuminator));
                            return;
                        }
                    }

                    if (_lineCount == 0)
                    {
                        #region No lines
                        switch (_shadowModel)
                        {
                            case ShadowModel.Mixed:
                            case ShadowModel.Deep:
                                // 6: In deep/mixed shadows, no lines yields a double-step downgraded level
                                switch (_level)
                                {
                                    case LightRange.VeryBright:
                                        workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.NearShadow, _illuminator));
                                        return;

                                    case LightRange.Bright:
                                        workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.FarShadow, _illuminator));
                                        return;
                                }
                                break;

                            case ShadowModel.Normal:
                            default:
                                // 2: In normal shadows, no lines yields a 1-step downgraded level
                                switch (_level)
                                {
                                    case LightRange.VeryBright:
                                    case LightRange.Bright:
                                        workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.NearShadow, _illuminator));
                                        return;

                                    case LightRange.NearShadow:
                                        workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.FarShadow, _illuminator));
                                        return;
                                }
                                break;
                        }
                        #endregion
                    }
                    else if ((_lineCount >= _maxLines / 3) && (_shadowModel == ShadowModel.Mixed))
                    {
                        // in mixed shadows, third lines yield the specified level
                        workSet.Feedback.Add(new IlluminateResult(_illuminator, _level, _illuminator));
                        return;
                    }
                    else if (_lineCount >= _maxLines / 2)
                    {
                        // 4: in deep shadows, half lines or more yields the specified level
                        workSet.Feedback.Add(new IlluminateResult(_illuminator, _level, _illuminator));
                        return;
                    }
                    else // somewhere between min and max / 2
                    {
                        // 5: In deep/mixed shadows, only some lines yields a 1-step downgraded level
                        switch (_level)
                        {
                            case LightRange.VeryBright:
                                workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.Bright, _illuminator));
                                return;

                            case LightRange.Bright:
                                workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.NearShadow, _illuminator));
                                return;

                            case LightRange.NearShadow:
                                workSet.Feedback.Add(new IlluminateResult(_illuminator, LightRange.FarShadow, _illuminator));
                                return;
                        }
                    }
                }

            }
            return;
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Illuminate);
            yield break;
        }
        #endregion

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
