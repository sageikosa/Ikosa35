using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Uzi.Visualize
{
    public class ThreeDimensionalBitArray
    {
        public ThreeDimensionalBitArray(int zBase, int yBase, int xBase, int zSize, int ySize, int xSize)
        {
            _ZBase = zBase;
            _YBase = yBase;
            _XBase = xBase;
            _ZRange = zSize;
            _YRange = ySize;
            _XRange = xSize;
            _MaxZ = (zSize - _ZBase) - 1;
            _MaxY = (ySize - _YBase) - 1;
            _MaxX = (xSize - _XBase) - 1;
            _Bits = new BitArray(zSize * ySize * xSize);
        }

        #region state
        private readonly BitArray _Bits;
        private readonly int _ZBase;
        private readonly int _YBase;
        private readonly int _XBase;
        private readonly int _ZRange;
        private readonly int _YRange;
        private readonly int _XRange;
        private readonly int _MaxZ;
        private readonly int _MaxY;
        private readonly int _MaxX;
        #endregion

        public int MinX => _XBase;
        public int MinY => _YBase;
        public int MinZ => _ZBase;

        public int MaxX => _MaxX;
        public int MaxY => _MaxY;
        public int MaxZ => _MaxZ;

        public int XRange => _XRange;
        public int YRange => _YRange;
        public int ZRange => _ZRange;

        public void Wipe(bool value)
        {
            _Bits.SetAll(value);
        }

        public bool InBounds(int z, int y, int x)
            => z >= MinZ && y >= MinY && x >= MinX
            && z <= MaxZ && y <= MaxY && x <= MaxX;

        private int CalcIndex(int z, int y, int x)
            => (x - MinX) + XRange * ((y - MinY) + YRange * z);

        public bool this[ICellLocation location]
        {
            get => this[location.Z, location.Y, location.X];
            set => this[location.Z, location.Y, location.X] = value;
        }

        public bool this[int z, int y, int x]
        {
            get
            {
                if (InBounds(z, y, x))
                    return _Bits.Get(CalcIndex(z, y, x));
                return false;
            }
            set
            {
                if (InBounds(z, y, x))
                    _Bits.Set(CalcIndex(z, y, x), value);
            }
        }
    }
}