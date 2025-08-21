using System.Collections.Specialized;

namespace Uzi.Visualize
{
    public struct CylinderParams
    {
        #region bit 32 vector management
        private static readonly BitVector32.Section _Face;
        private static readonly BitVector32.Section _SegCount;
        private static readonly BitVector32.Section _Style;
        private static readonly BitVector32.Section _Pediment;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static CylinderParams()
        {
            _Face = BitVector32.CreateSection(6);
            _SegCount = BitVector32.CreateSection(15, _Face);
            _Style = BitVector32.CreateSection(3, _SegCount);
            _Pediment = BitVector32.CreateSection(30, _Style);
        }
        #endregion

        public CylinderParams(uint value)
        {
            _Vector = new BitVector32((int)value);
        }

        private BitVector32 _Vector;

        public uint Value { get { return (uint)_Vector.Data; } }

        // cylinder
        public AnchorFace AnchorFace
        {
            get { return (AnchorFace)_Vector[_Face]; }
            set { _Vector[_Face] = (int)value; }
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

        public int PedimentUnits
        {
            get { return _Vector[_Pediment]; }
            set
            {
                if ((value <= 60) && (value >= 0))
                {
                    _Vector[_Pediment] = value;
                }
            }
        }

        public double Pediment
        {
            get { return (PedimentUnits * 5d) / 60d; }
            set
            {
                if ((value <= 5d) && (value >= 0d))
                {
                    PedimentUnits = (int)(value * 60d / 5d);
                }
            }
        }
    }
}
