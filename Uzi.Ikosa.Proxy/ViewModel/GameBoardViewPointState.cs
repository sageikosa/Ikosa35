using System;
using System.Collections.Specialized;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public struct GameBoardViewPointState
    {
        #region bit 32 vector management
        private static readonly int _Above;
        private static readonly int _Gaze;
        private static readonly BitVector32.Section _Heading;
        private static readonly BitVector32.Section _Width;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static GameBoardViewPointState()
        {
            _Above = BitVector32.CreateMask();
            _Gaze = BitVector32.CreateMask(_Above);
            _Heading = BitVector32.CreateSection(7, BitVector32.CreateSection(3));
            _Width = BitVector32.CreateSection(255, _Heading);
        }
        #endregion

        public GameBoardViewPointState(uint value)
        {
            _Vector = new BitVector32((int)value);
            if (WidthCells < 10)
                WidthCells = 10;
        }

        private BitVector32 _Vector;

        public uint Value => (uint)_Vector.Data;

        public int Heading
        {
            get => _Vector[_Heading];
            set => _Vector[_Heading] = value;
        }

        public bool Above
        {
            get => _Vector[_Above];
            set => _Vector[_Above] = value;
        }

        public bool Gaze
        {
            get => _Vector[_Gaze];
            set => _Vector[_Gaze] = value;
        }

        public int WidthCells
        {
            get => _Vector[_Width];
            set => _Vector[_Width] = Math.Min(255, Math.Max(value, 10));
        }

        public double Width
        {
            get => _Vector[_Width] * 5d;
            set => _Vector[_Width] = (int)(value / 5d);
        }
    }
}
