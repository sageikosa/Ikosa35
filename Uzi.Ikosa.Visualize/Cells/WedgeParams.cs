using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize
{
    public struct WedgeParams
    {
        #region bit 32 vector management
        private static readonly BitVector32.Section _Axis;
        private static readonly int _Flip;
        private static readonly int _PriInv;
        private static readonly int _SecInv;
        private static readonly BitVector32.Section _SegCount;
        private static readonly BitVector32.Section _Style;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static WedgeParams()
        {
            _Axis = BitVector32.CreateSection(2);       // bits 0-1
            _Flip = BitVector32.CreateMask(2);          // bit 2
            _PriInv = BitVector32.CreateMask(_Flip);    // bit 3
            _SecInv = BitVector32.CreateMask(_PriInv);  // bit 4
            _SegCount = BitVector32.CreateSection(7, BitVector32.CreateSection(31)); // reserved room for first 5 bits
            _Style = BitVector32.CreateSection(3, _SegCount);
        }
        #endregion

        public WedgeParams(uint value)
        {
            _Vector = new BitVector32((int)value);
        }

        private BitVector32 _Vector;

        public uint Value { get { return (uint)_Vector.Data; } }

        // wedge params
        public Axis Axis
        {
            get { return (Axis)_Vector[_Axis]; }
            set { _Vector[_Axis] = (int)value; }
        }
        public bool FlipOffsets
        {
            get { return _Vector[_Flip]; }
            set { _Vector[_Flip] = value; }
        }
        public bool InvertPrimary
        {
            get { return _Vector[_PriInv]; }
            set { _Vector[_PriInv] = value; }
        }
        public bool InvertSecondary
        {
            get { return _Vector[_SecInv]; }
            set { _Vector[_SecInv] = value; }
        }
        public byte SegmentCount
        {
            get { return (byte)_Vector[_SegCount]; }
            set { _Vector[_SegCount] = value; }
        }
        public CylinderStyle Style
        {
            get { return (CylinderStyle)_Vector[_Style]; }
            set { _Vector[_Style] = (byte)value; }
        }

        public AnchorFace PrimarySnap
        {
            get
            {
                switch (Axis)
                {
                    case Axis.Z:
                        return InvertPrimary ? AnchorFace.XHigh : AnchorFace.XLow;
                    case Axis.Y:
                        return InvertPrimary ? AnchorFace.ZHigh : AnchorFace.ZLow;
                    case Axis.X:
                    default:
                        return InvertPrimary ? AnchorFace.YHigh : AnchorFace.YLow;
                }
            }
        }

        public AnchorFace SecondarySnap
        {
            get
            {
                switch (Axis)
                {
                    case Axis.Z:
                        return InvertSecondary ? AnchorFace.YHigh : AnchorFace.YLow;
                    case Axis.Y:
                        return InvertSecondary ? AnchorFace.XHigh : AnchorFace.XLow;
                    case Axis.X:
                    default:
                        return InvertSecondary ? AnchorFace.ZHigh : AnchorFace.ZLow;
                }
            }
        }

        public double PrimaryOffset(double offset1, double offset2)
        {
            double _factor = InvertPrimary ? -1 : 1;
            if (FlipOffsets)
                return offset2 * _factor;
            else
                return offset1 * _factor;
        }

        public double SecondaryOffset(double offset1, double offset2)
        {
            double _factor = InvertSecondary ? -1 : 1;
            if (FlipOffsets)
                return offset1 * _factor;
            else
                return offset2 * _factor;
        }
    }
}
