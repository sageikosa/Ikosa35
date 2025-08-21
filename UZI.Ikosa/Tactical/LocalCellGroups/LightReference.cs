using System;
using System.Linq;
using Uzi.Ikosa.Senses;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LightReference
    {
        #region construction
        public LightReference(IIllumination light, LocalLink link, Point3D point, Vector3D normal)
        {
            _Light = light;

            // attenuation values
            _Vector = (point - light.InteractionPoint3D(point));

            // attenuation diminishes acutely with proximity
            var _dist = _Vector.Length;
            var _impact = light is LocalLinkLight ? 1.125d : Math.Min(Math.Max(_dist - 5d, 0d) / 15d, 1d);

            // otherwise based mostly on angles
            // TODO: consider altering the angle for highly proximal operations
            _Angle = _impact <= 1
                ? Math.Min(Vector3D.AngleBetween(_Vector, normal), 90) * _impact
                : Math.Min(Vector3D.AngleBetween(_Vector, normal) * _impact, 90);

            // light results
            _Solar = (light.SolarLeft(point) - _dist) * link.AllowLightFactor;
            if (_Solar < 0)
            {
                _Solar = 0;
            }

            _VBright = (light.VeryBrightLeft(point) - _dist) * link.AllowLightFactor;
            if (_VBright < 0)
            {
                _VBright = 0;
            }

            _Bright = (light.BrightLeft(point) - _dist) * link.AllowLightFactor;
            if (_Bright < 0)
            {
                _Bright = 0;
            }

            _Shadow = (light.ShadowyLeft(point) - _dist) * link.AllowLightFactor;
            if (_Shadow < 0)
            {
                _Shadow = 0;
            }

            _FarShadow = (light.FarShadowyLeft(point) - _dist) * link.AllowLightFactor;
            if (_FarShadow < 0)
            {
                _FarShadow = 0;
            }

            // line of effect and "normal" shadow determination of light level...
            var _lightUp = new Illuminate(null, link, light);
            var _lightInteract = new Interaction(null, light.LightHandler, link.Holder, _lightUp);
            light.LightHandler.HandleInteraction(_lightInteract);
            if (_lightInteract.Feedback.Count != 0)
            {
                var _result = _lightInteract.Feedback.OfType<IlluminateResult>().FirstOrDefault();
                if (_result != null)
                {
                    // only if very bright light continued to target
                    if (_result.Level < LightRange.Solar)
                    {
                        _Solar = 0;
                    }

                    if (_result.Level < LightRange.VeryBright)
                    {
                        _VBright = 0;
                    }

                    switch (_result.Level)
                    {
                        case LightRange.OutOfRange:
                        case LightRange.ExtentBoost:
                            _Bright = 0;
                            _Shadow = 0;
                            _FarShadow = 0;
                            break;

                        case LightRange.FarShadow:
                        case LightRange.FarBoost:
                            // should have had shadowy?
                            if (_Shadow > 0)
                            {
                                // should have had bright?
                                if (_Bright > 0)
                                {
                                    // blocked, double step-down...
                                    _FarShadow = _Bright;
                                }
                                else
                                {
                                    // blocked, single step-down
                                    _FarShadow = _Shadow;
                                }

                                // when in far shadows, not projecting anything better
                                _Shadow = 0;
                                _Bright = 0;
                                _VBright = 0;
                            }
                            break;

                        case LightRange.NearShadow:
                        case LightRange.NearBoost:
                            // should have had bright?
                            if (_Bright > 0)
                            {
                                // blocked, single step-down
                                _FarShadow = _Shadow;
                                _Shadow = _Bright;
                            }

                            // when in near shadows, not projecting anything better
                            _Bright = 0;
                            _VBright = 0;
                            break;
                    }
                }
                else
                {
                    // NO feedback!  No light...?
                    _Bright = 0;
                    _VBright = 0;
                    _Shadow = 0;
                    _FarShadow = 0;
                }
            }
        }
        #endregion

        #region state
        private double _Solar;
        private double _VBright;
        private double _Bright; // BRIGHT DISTANCE REMAINING
        private double _Shadow; // SHADOW DISTANCE REMAINING
        private double _FarShadow; // FAR SHADOW DISTANCE REMAINING
        private Vector3D _Vector;
        private double _Angle;
        private IIllumination _Light;
        #endregion

        public double LinkAngle => _Angle;
        public Vector3D Vector => _Vector;
        public double SolarRange => _Solar;
        public double VeryBrightRange => _VBright;
        public double BrightRange => _Bright;
        public double ShadowyRange => _Shadow;
        public double FarShadowRange => _FarShadow;
        public IIllumination IIlumination => _Light;

        public bool IsProvidingLight
            => (_Solar > 0) || (_VBright > 0) || (_Bright > 0) || (_Shadow > 0) || (_FarShadow > 0);

    }
}