using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Uzi.Visualize
{
    public struct SliverSlopeParams
    {
        #region bit 32 vector management
        private static readonly BitVector32.Section _Axis;
        private static readonly BitVector32.Section _Flip;
        private static readonly BitVector32.Section _Slope;
        private static readonly BitVector32.Section _Offset;
        private static readonly BitVector32.Section _Offset2;
        private static readonly int _ZD;
        private static readonly int _YD;
        private static readonly int _XD;
        private static readonly int _ZLYL;
        private static readonly int _ZLYH;
        private static readonly int _ZLXL;
        private static readonly int _ZLXH;
        private static readonly int _ZHYL;
        private static readonly int _ZHYH;
        private static readonly int _ZHXL;
        private static readonly int _ZHXH;
        private static readonly int _YLXL;
        private static readonly int _YLXH;
        private static readonly int _YHXL;
        private static readonly int _YHXH;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static SliverSlopeParams()
        {
            _Axis = BitVector32.CreateSection(2);
            _Flip = BitVector32.CreateSection(1, _Axis);
            _Slope = BitVector32.CreateSection(2, _Flip);
            _Offset = BitVector32.CreateSection(60, _Slope);
            _Offset2 = BitVector32.CreateSection(60, _Offset);
            _ZD = BitVector32.CreateMask(65536);
            _YD = BitVector32.CreateMask(_ZD);
            _XD = BitVector32.CreateMask(_YD);
            _ZLYL = BitVector32.CreateMask(_XD);
            _ZLYH = BitVector32.CreateMask(_ZLYL);
            _ZLXL = BitVector32.CreateMask(_ZLYH);
            _ZLXH = BitVector32.CreateMask(_ZLXL);
            _ZHYL = BitVector32.CreateMask(_ZLXH);
            _ZHYH = BitVector32.CreateMask(_ZHYL);
            _ZHXL = BitVector32.CreateMask(_ZHYH);
            _ZHXH = BitVector32.CreateMask(_ZHXL);
            _YLXL = BitVector32.CreateMask(_ZHXH);
            _YLXH = BitVector32.CreateMask(_YLXL);
            _YHXL = BitVector32.CreateMask(_YLXH);
            _YHXH = BitVector32.CreateMask(_YHXL);
        }
        #endregion

        public SliverSlopeParams(uint value)
        {
            _Vector = new BitVector32((int)value);
        }

        private BitVector32 _Vector;

        public uint Value { get { return (uint)_Vector.Data; } }

        // sliver
        public Axis Axis
        {
            get { return (Axis)_Vector[_Axis]; }
            set { _Vector[_Axis] = (int)value; }
        }
        public bool Flip
        {
            get { return (_Vector[_Flip] > 0); }
            set { _Vector[_Flip] = value ? 1 : 0; }
        }

        public int OffsetUnits
        {
            get { return _Vector[_Offset]; }
            set
            {
                if ((value <= 60) && (value >= 0))
                {
                    _Vector[_Offset] = value;
                }
            }
        }

        public double Offset
        {
            get { return (OffsetUnits * 5d) / 60d; }
            set
            {
                if ((value <= 5d) && (value >= 0d))
                {
                    OffsetUnits = (int)(value * 60d / 5d);
                }
            }
        }

        // slope
        public Axis SlopeAxis
        {
            get { return (Axis)_Vector[_Slope]; }
            set { _Vector[_Slope] = (int)value; }
        }

        public int HiOffsetUnits
        {
            get { return _Vector[_Offset2]; }
            set
            {
                if ((value <= 60) && (value >= 0))
                {
                    _Vector[_Offset2] = value;
                }
            }
        }

        public double HiOffset
        {
            get { return (HiOffsetUnits * 5d) / 60d; }
            set
            {
                if ((value <= 5d) && (value >= 0d))
                {
                    HiOffsetUnits = (int)(value * 60d / 5d);
                }
            }
        }

        public double LoFlippableOffset
            => Flip ? 5 - Offset : Offset;

        public double HiFlippableOffset
            => Flip ? 5 - HiOffset : HiOffset;

        // doubler flags
        public bool ZDoubler { get { return _Vector[_ZD]; } set { _Vector[_ZD] = value; } }
        public bool YDoubler { get { return _Vector[_YD]; } set { _Vector[_YD] = value; } }
        public bool XDoubler { get { return _Vector[_XD]; } set { _Vector[_XD] = value; } }

        // edge flags
        public bool ZLoYLo { get { return _Vector[_ZLYL]; } set { _Vector[_ZLYL] = value; } }
        public bool ZLoYHi { get { return _Vector[_ZLYH]; } set { _Vector[_ZLYH] = value; } }
        public bool ZLoXLo { get { return _Vector[_ZLXL]; } set { _Vector[_ZLXL] = value; } }
        public bool ZLoXHi { get { return _Vector[_ZLXH]; } set { _Vector[_ZLXH] = value; } }
        public bool ZHiYLo { get { return _Vector[_ZHYL]; } set { _Vector[_ZHYL] = value; } }
        public bool ZHiYHi { get { return _Vector[_ZHYH]; } set { _Vector[_ZHYH] = value; } }
        public bool ZHiXLo { get { return _Vector[_ZHXL]; } set { _Vector[_ZHXL] = value; } }
        public bool ZHiXHi { get { return _Vector[_ZHXH]; } set { _Vector[_ZHXH] = value; } }
        public bool YLoXLo { get { return _Vector[_YLXL]; } set { _Vector[_YLXL] = value; } }
        public bool YLoXHi { get { return _Vector[_YLXH]; } set { _Vector[_YLXH] = value; } }
        public bool YHiXLo { get { return _Vector[_YHXL]; } set { _Vector[_YHXL] = value; } }
        public bool YHiXHi { get { return _Vector[_YHXH]; } set { _Vector[_YHXH] = value; } }

        #region public bool HasEdges(AnchorFace face)
        public bool HasEdges(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.ZLow: return ZLoYLo || ZLoYHi || ZLoXLo || ZLoXHi;
                case AnchorFace.YLow: return ZLoYLo || ZHiYLo || YLoXLo || YLoXHi;
                case AnchorFace.XLow: return ZLoXLo || ZHiXLo || YLoXLo || YHiXLo;
                case AnchorFace.ZHigh: return ZHiYLo || ZHiYHi || ZHiXLo || ZHiXHi;
                case AnchorFace.YHigh: return ZLoYHi || ZHiYHi || YHiXLo || YHiXHi;
                case AnchorFace.XHigh:
                default: return ZLoXHi || ZHiXHi || YLoXHi || YHiXHi;
            }
        }
        #endregion

        #region public IEnumerable<EdgeStruc> GetSliverEdges(ICellEdge edge)
        public IEnumerable<EdgeStruc> GetSliverEdges(ICellEdge edge)
        {
            var _offFace = Flip ? Axis.GetHighFace() : Axis.GetLowFace();
            var _param = this;
            double _offCalc(AnchorFace face) => (face == _offFace) ? _param.Offset : 0d;

            if (ZLoYLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.YLow, _offCalc(AnchorFace.YLow), _offCalc(AnchorFace.ZLow));
            }

            if (ZLoYHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.YHigh, _offCalc(AnchorFace.YHigh), _offCalc(AnchorFace.ZLow));
            }

            if (ZLoXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.XLow, _offCalc(AnchorFace.ZLow), _offCalc(AnchorFace.XLow));
            }

            if (ZLoXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.XHigh, _offCalc(AnchorFace.ZLow), _offCalc(AnchorFace.XHigh));
            }

            if (ZHiYLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.YLow, _offCalc(AnchorFace.YLow), _offCalc(AnchorFace.ZHigh));
            }

            if (ZHiYHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.YHigh, _offCalc(AnchorFace.YHigh), _offCalc(AnchorFace.ZHigh));
            }

            if (ZHiXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.XLow, _offCalc(AnchorFace.ZHigh), _offCalc(AnchorFace.XLow));
            }

            if (ZHiXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.XHigh, _offCalc(AnchorFace.ZHigh), _offCalc(AnchorFace.XHigh));
            }

            if (YLoXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YLow | AnchorFaceList.XLow, _offCalc(AnchorFace.XLow), _offCalc(AnchorFace.YLow));
            }

            if (YLoXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YLow | AnchorFaceList.XHigh, _offCalc(AnchorFace.XHigh), _offCalc(AnchorFace.YLow));
            }

            if (YHiXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YHigh | AnchorFaceList.XLow, _offCalc(AnchorFace.XLow), _offCalc(AnchorFace.YHigh));
            }

            if (YHiXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YHigh | AnchorFaceList.XHigh, _offCalc(AnchorFace.XHigh), _offCalc(AnchorFace.YHigh));
            }

            yield break;
        }
        #endregion

        #region public IEnumerable<EdgeStruc> GetSlopeEdges(ICellEdge edge)
        public IEnumerable<EdgeStruc> GetSlopeEdges(ICellEdge edge)
        {
            var _offFace = Flip ? Axis.GetHighFace() : Axis.GetLowFace();
            var _param = this;
            double _offCalc(AnchorFace face, AnchorFace other)
                => (face == _offFace)
                ? ((other.GetAxis() == _param.SlopeAxis)
                    ? (other.IsLowFace() ? _param.Offset : _param.HiOffset)
                    : Math.Max(_param.Offset, _param.HiOffset))
                : 0d;

            if (ZLoYLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.YLow, _offCalc(AnchorFace.YLow, AnchorFace.ZLow), _offCalc(AnchorFace.ZLow, AnchorFace.YLow));
            }

            if (ZLoYHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.YHigh, _offCalc(AnchorFace.YHigh, AnchorFace.ZLow), _offCalc(AnchorFace.ZLow, AnchorFace.YHigh));
            }

            if (ZLoXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.XLow, _offCalc(AnchorFace.ZLow, AnchorFace.XLow), _offCalc(AnchorFace.XLow, AnchorFace.ZLow));
            }

            if (ZLoXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZLow | AnchorFaceList.XHigh, _offCalc(AnchorFace.ZLow, AnchorFace.XHigh), _offCalc(AnchorFace.XHigh, AnchorFace.ZLow));
            }

            if (ZHiYLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.YLow, _offCalc(AnchorFace.YLow, AnchorFace.ZHigh), _offCalc(AnchorFace.ZHigh, AnchorFace.YLow));
            }

            if (ZHiYHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.YHigh, _offCalc(AnchorFace.YHigh, AnchorFace.ZHigh), _offCalc(AnchorFace.ZHigh, AnchorFace.YHigh));
            }

            if (ZHiXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.XLow, _offCalc(AnchorFace.ZHigh, AnchorFace.XLow), _offCalc(AnchorFace.XLow, AnchorFace.ZHigh));
            }

            if (ZHiXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.ZHigh | AnchorFaceList.XHigh, _offCalc(AnchorFace.ZHigh, AnchorFace.XHigh), _offCalc(AnchorFace.XHigh, AnchorFace.ZHigh));
            }

            if (YLoXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YLow | AnchorFaceList.XLow, _offCalc(AnchorFace.XLow, AnchorFace.YLow), _offCalc(AnchorFace.YLow, AnchorFace.XLow));
            }

            if (YLoXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YLow | AnchorFaceList.XHigh, _offCalc(AnchorFace.XHigh, AnchorFace.YLow), _offCalc(AnchorFace.YLow, AnchorFace.XHigh));
            }

            if (YHiXLo)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YHigh | AnchorFaceList.XLow, _offCalc(AnchorFace.XLow, AnchorFace.YHigh), _offCalc(AnchorFace.YHigh, AnchorFace.XLow));
            }

            if (YHiXHi)
            {
                yield return new EdgeStruc(edge, AnchorFaceList.YHigh | AnchorFaceList.XHigh, _offCalc(AnchorFace.XHigh, AnchorFace.YHigh), _offCalc(AnchorFace.YHigh, AnchorFace.XHigh));
            }

            yield break;
        }
        #endregion
    }
}
