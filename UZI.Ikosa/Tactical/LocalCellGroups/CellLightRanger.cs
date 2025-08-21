using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public struct CellLightRanger
    {
        public CellLightRanger(IIllumination light, CellPosition cellLocation)
        {
            _Light = light;
            _Cell = cellLocation;
            _Point3D = new Point3D();
            _UsePoint = false;
            _Distance = Uzi.Visualize.IGeometricHelper.Distance(_Light.InteractionPoint3D(cellLocation), _Cell);
            _BrightLeft = null;
            _ExtentBoost = null;
            _FarBoost = null;
            _FarShadowLeft = null;
            _NearBoost = null;
            _ShadowLeft = null;
            _VeryBrightLeft = null;
        }

        public CellLightRanger(IIllumination light, Point3D point3D)
        {
            _Light = light;
            _Cell = new CellPosition();
            _Point3D = point3D;
            _UsePoint = true;
            _Distance = (_Light.InteractionPoint3D(point3D) - point3D).Length;
            _BrightLeft = null;
            _ExtentBoost = null;
            _FarBoost = null;
            _FarShadowLeft = null;
            _NearBoost = null;
            _ShadowLeft = null;
            _VeryBrightLeft = null;
        }

        #region private data
        private IIllumination _Light;
        private bool _UsePoint;
        private CellPosition _Cell;
        private Point3D _Point3D;
        private double? _VeryBrightLeft;
        private double? _BrightLeft;
        private double? _ShadowLeft;
        private double? _FarShadowLeft;
        private double? _NearBoost;
        private double? _FarBoost;
        private double? _ExtentBoost;
        private double _Distance;
        #endregion

        #region public LightRange CurrentRange { get; }
        public LightRange CurrentRange
        {
            get
            {
                var _max = _Light.MaximumLight;
                if (_max >= LightRange.VeryBright)
                {
                    if (_Distance <= VeryBrightLeft)
                    {
                        return _max;
                    }
                }
                if (_max >= LightRange.Bright)
                {
                    if (_Distance <= BrightLeft)
                    {
                        return LightRange.Bright;
                    }
                    else if (_Distance <= NearBoostLeft)
                    {
                        return LightRange.NearBoost;
                    }
                }
                if (_max >= LightRange.NearShadow)
                {
                    if (_Distance <= ShadowyLeft)
                    {
                        return LightRange.NearShadow;
                    }
                    else if (_Distance <= FarBoostLeft)
                    {
                        return LightRange.FarBoost;
                    }
                }
                if (_max >= LightRange.FarShadow)
                {
                    if (_Distance <= FarShadowyLeft)
                    {
                        return LightRange.FarShadow;
                    }
                    else if (_Distance <= ExtentBoostLeft)
                    {
                        return LightRange.ExtentBoost;
                    }
                }
                return LightRange.OutOfRange;
            }
        }
        #endregion

        #region public double VeryBrightLeft { get; }
        public double VeryBrightLeft
        {
            get
            {
                if (!_VeryBrightLeft.HasValue)
                {
                    _VeryBrightLeft = _UsePoint 
                        ? _Light.VeryBrightLeft(_Point3D) 
                        : _Light.VeryBrightLeft(_Cell);
                }
                return _VeryBrightLeft.Value;
            }
        }
        #endregion

        #region public double BrightLeft { get; }
        public double BrightLeft
        {
            get
            {
                if (!_BrightLeft.HasValue)
                {
                    _BrightLeft = _UsePoint
                        ? _Light.BrightLeft(_Point3D)
                        : _Light.BrightLeft(_Cell);
                }
                return _BrightLeft.Value;
            }
        }
        #endregion

        #region public double ShadowyLeft { get; }
        public double ShadowyLeft
        {
            get
            {
                if (!_ShadowLeft.HasValue)
                {
                    _ShadowLeft = _UsePoint
                        ? _Light.ShadowyLeft(_Point3D)
                        : _Light.ShadowyLeft(_Cell);
                }
                return _ShadowLeft.Value;
            }
        }
        #endregion

        #region public double FarShadowyLeft { get; }
        public double FarShadowyLeft
        {
            get
            {
                if (!_FarShadowLeft.HasValue)
                {
                    _FarShadowLeft = _UsePoint
                        ? _Light.FarShadowyLeft(_Point3D)
                        : _Light.FarShadowyLeft(_Cell);
                }
                return _FarShadowLeft.Value;
            }
        }
        #endregion

        #region public double NearBoostLeft { get; }
        public double NearBoostLeft
        {
            get
            {
                if (!_NearBoost.HasValue)
                {
                    _NearBoost = _UsePoint
                        ? _Light.NearBoostLeft(_Point3D)
                        : _Light.NearBoostLeft(_Cell);
                }
                return _NearBoost.Value;
            }
        }
        #endregion

        #region public double FarBoostLeft { get; }
        public double FarBoostLeft
        {
            get
            {
                if (!_FarBoost.HasValue)
                {
                    _FarBoost = _UsePoint
                        ? _Light.FarBoostLeft(_Point3D)
                        : _Light.FarBoostLeft(_Cell);
                }
                return _FarBoost.Value;
            }
        }
        #endregion

        #region public double ExtentBoostLeft { get; }
        public double ExtentBoostLeft
        {
            get
            {
                if (!_ExtentBoost.HasValue)
                {
                    _ExtentBoost = _UsePoint
                        ? _Light.ExtentBoostLeft(_Point3D)
                        : _Light.ExtentBoostLeft(_Cell);
                }
                return _ExtentBoost.Value;
            }
        }
        #endregion

        public static LightRange AdjustRange(LightRange expected, int effectCount, bool deepShadows)
        {
            if (deepShadows)
            {
                switch (effectCount)
                {
                    case 5:
                        // no adjustment
                        return expected;

                    case 4:
                    case 3:
                        // one level adjustment
                        if (expected > LightRange.OutOfRange)
                        {
                            return expected - 1;
                        }
                        else
                        {
                            return LightRange.OutOfRange;
                        }

                    case 2:
                    case 1:
                        // two level adjustment
                        if (expected > LightRange.ExtentBoost)
                        {
                            return expected - 2;
                        }
                        else
                        {
                            return LightRange.OutOfRange;
                        }

                    default:
                        // nothing
                        return LightRange.OutOfRange;
                }
            }
            else
            {
                switch (effectCount)
                {
                    case 2:
                        // no adjustment
                        return expected;

                    case 1:
                        // one level adjustment
                        if (expected > LightRange.OutOfRange)
                        {
                            return expected - 1;
                        }
                        else
                        {
                            return LightRange.OutOfRange;
                        }

                    default:
                        // nothing
                        return LightRange.OutOfRange;
                }
            }
        }
    }
}
