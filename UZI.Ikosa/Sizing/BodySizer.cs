using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    // TODO: more like ItemSizer...work out dynamics
    [Serializable]
    public class BodySizer : Sizer
    {
        #region construction
        public BodySizer(Size size)
            : base(size)
        {
            _BaseSize = size;
            _SizeDelta = new DeltaPtr(size.SizeDelta);
            _OpposedDelta = new DeltaPtr(size.OpposedDelta);
            _HideDelta = new DeltaPtr(size.HideDelta);
        }
        #endregion

        #region data
        private Size _BaseSize;
        protected DeltaPtr _SizeDelta;
        protected DeltaPtr _OpposedDelta;
        protected DeltaPtr _HideDelta;
        #endregion

        /// <summary>Defined size (for base weight and height)</summary>
        public Size BaseSize => _BaseSize;

        #region public double WeightFactor { get; }
        /// <summary>Weight factor based on size variation from base size</summary>
        public double WeightFactor
        {
            get
            {
                // calculate change steps
                var _change = Size.Order - BaseSize.Order;
                if (_change != 0)
                {
                    // powers of 2 to keep in line with item and strength changes
                    return Math.Pow(2, _change);
                }
                return 1;
            }
        }
        #endregion

        #region public double SpatialFactor { get; }
        /// <summary>Height factor based on size variation from base height</summary>
        public double SpatialFactor
        {
            get
            {
                double _factor = 1;
                if (Size.Order > BaseSize.Order)
                {
                    // increasing size
                    for (var _f = BaseSize.Order; _f < Size.Order; _f++)
                    {
                        switch (_f + 1)
                        {
                            // huge and larger have a diminishing height increase
                            case 2:
                            case 3:
                            case 4:
                                _factor *= ((_f + 1) / _f);
                                break;

                            // if sizing to anything less than huge, simply double
                            default:
                                _factor *= 2;
                                break;
                        }
                    }
                }
                else
                {
                    // decreasing size
                    for (var _f = BaseSize.Order; _f > Size.Order; _f--)
                    {
                        switch (_f)
                        {
                            case 2:
                            case 3:
                            case 4:
                                _f /= ((_f + 1) / _f);
                                break;

                            default:
                                _factor /= 2;
                                break;
                        }
                    }
                }
                return _factor;
            }
        }
        #endregion

        public IModifier SizeModifier => _SizeDelta;
        public IModifier OpposedModifier => _OpposedDelta;
        public IModifier HideModifier => _HideDelta;

        protected override void OnSizeChange()
        {
            _SizeDelta.CurrentModifier = _Size.SizeDelta;
            _OpposedDelta.CurrentModifier = _Size.OpposedDelta;
            _HideDelta.CurrentModifier = _Size.HideDelta;
        }
    }
}
