using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Uzi.Ikosa.Proxy.VisualizationSvc;

namespace Uzi.Ikosa.Proxy
{
    public struct RoomTrackerKey
    {
        #region bit 32 vector management
        private static readonly BitVector32.Section _ZSect;
        private static readonly BitVector32.Section _YSect;
        private static readonly BitVector32.Section _XSect;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static RoomTrackerKey()
        {
            _XSect = BitVector32.CreateSection(1023);
            _YSect = BitVector32.CreateSection(1023, _XSect);
            _ZSect = BitVector32.CreateSection(1023, _YSect);
        }
        #endregion

        private const int THICK = 5;
        private readonly BitVector32 _Vector;

        // ranges
        // Axis  Cell-Low  Cell-High  Idx-Low  Idx-High  Idx-Range
        // Z     -2560     2559       -512     511       1024
        // Y     -2560     2559       -512     511       1024
        // X     -2560     2559       -512     511       1024

        private static int _GetIndex(int val)
            => val < 0
            ? ((val + 1) / THICK) - 1
            : (val / THICK);

        public RoomTrackerKey(int z, int y, int x)
        {
            _Vector = new BitVector32();
            _Vector[_ZSect] = _GetIndex(z) + 512;
            _Vector[_YSect] = _GetIndex(y) + 512;
            _Vector[_XSect] = _GetIndex(x) + 512;
        }

        public RoomTrackerKey(BitVector32 vector)
        {
            _Vector = vector;
        }

        public RoomTrackerKey(int data)
        {
            _Vector = new BitVector32(data);
        }

        public int Z { get => _Vector[_ZSect]; }
        public int Y { get => _Vector[_YSect]; }
        public int X { get => _Vector[_XSect]; }

        public override int GetHashCode()
            => _Vector.GetHashCode();

        public override bool Equals(object obj)
            => _Vector.GetHashCode() == obj.GetHashCode();

        public static IEnumerable<RoomTrackerKey> GetKeys(RoomViewModel room)
        {
            var _loZ = _GetIndex(room.LowerZ);
            var _hiZ = _GetIndex(room.UpperZ);
            for (var _idxZ = _loZ; _idxZ <= _hiZ; _idxZ++)
            {
                var _loY = _GetIndex(room.LowerY);
                var _hiY = _GetIndex(room.UpperY);
                for (var _idxY = _loY; _idxY <= _hiY; _idxY++)
                {
                    var _loX = _GetIndex(room.LowerX);
                    var _hiX = _GetIndex(room.UpperX);
                    for (var _idxX = _loX; _idxX <= _hiX; _idxX++)
                    {
                        var _vector = new BitVector32();
                        _vector[_ZSect] = _idxZ + 512;
                        _vector[_YSect] = _idxY + 512;
                        _vector[_XSect] = _idxX + 512;
                        yield return new RoomTrackerKey(_vector);
                    }

                }
            }
            yield break;
        }
    }
}
