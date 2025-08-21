using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public struct HedralGrip
    {
        public HedralGrip(bool block)
        {
            GripValue = block ? ulong.MaxValue : 0;
        }

        public HedralGrip(Axis moveAxis, AnchorFace blockSnap, double amount)
        {
            GripValue = GetSliver(moveAxis, blockSnap, amount);
        }

        public HedralGrip(TriangleCorner corner)
        {
            GripValue = GetDiagonal(corner);
        }

        public HedralGrip(Axis moveAxis, AnchorFace blockSnap, double loOff, double hiOff)
        {
            GripValue = GetSlope(moveAxis, blockSnap, loOff, hiOff);
        }

        public HedralGrip(double primeOff, double secondOff, bool flip)
        {
            GripValue = GetWedge(primeOff, secondOff, flip);
        }

        public bool HasAny => GripValue > 0;
        public bool HasAll => GripValue == ulong.MaxValue;

        public HedralGrip Invert() => new HedralGrip { GripValue = GripValue ^ ulong.MaxValue };
        public HedralGrip Intersect(HedralGrip other) => new HedralGrip { GripValue = GripValue & other.GripValue };
        public HedralGrip Union(HedralGrip other) => new HedralGrip { GripValue = GripValue | other.GripValue };

        public ulong GripValue { get; set; }

        public override int GetHashCode()
            => GripValue.GetHashCode();

        public override bool Equals(object obj)
            => GripValue.Equals(obj);

        public byte GripCount()
        {
            // adapted from: https://stackoverflow.com/questions/2709430
            var _grip = GripValue - ((GripValue >> 1) & 0x5555555555555555);
            _grip = (_grip & 0x3333333333333333) + ((_grip >> 2) & 0x3333333333333333);
            return (byte)((((_grip + (_grip >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
        }


        #region public static ulong GetSliver(Axis axis, AnchorFace blockSnap, double amount)
        private static readonly ulong[] _RightSliverLUT =
        {
            0,
            0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001,
            0b00000011_00000011_00000011_00000011_00000011_00000011_00000011_00000011,
            0b00000111_00000111_00000111_00000111_00000111_00000111_00000111_00000111,
            0b00001111_00001111_00001111_00001111_00001111_00001111_00001111_00001111,
            0b00011111_00011111_00011111_00011111_00011111_00011111_00011111_00011111,
            0b00111111_00111111_00111111_00111111_00111111_00111111_00111111_00111111,
            0b01111111_01111111_01111111_01111111_01111111_01111111_01111111_01111111,
            0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111
        };

        private static ulong BottomSliver(byte units)
                => units >= 8
                ? ulong.MaxValue
                : ((ulong)1 << (units * 8)) - 1;

        private static ulong TopSliver(byte units)
                => BottomSliver((byte)(8 - units)) ^ ulong.MaxValue;

        private static ulong RightSliver(byte units)
        {
            return _RightSliverLUT[units];
        }

        private static ulong LeftSliver(byte units)
            => RightSliver((byte)(8 - units)) ^ ulong.MaxValue;

        private static ulong GetSliver(Axis moveAxis, AnchorFace blockSnap, double amount)
        {
            var _units = Math.Min((byte)Math.Round(amount * 8 / 5), (byte)8);
            switch (moveAxis)
            {
                case Axis.X:
                    switch (blockSnap)
                    {
                        case AnchorFace.YLow:
                            return LeftSliver(_units);
                        case AnchorFace.YHigh:
                            return RightSliver(_units);
                        case AnchorFace.ZLow:
                            return BottomSliver(_units);
                        case AnchorFace.ZHigh:
                            return TopSliver(_units);
                    }
                    return 0;

                case Axis.Y:
                    switch (blockSnap)
                    {
                        case AnchorFace.XLow:
                            return BottomSliver(_units);
                        case AnchorFace.XHigh:
                            return TopSliver(_units);
                        case AnchorFace.ZLow:
                            return LeftSliver(_units);
                        case AnchorFace.ZHigh:
                            return RightSliver(_units);
                    }
                    return 0;

                case Axis.Z:
                default:
                    switch (blockSnap)
                    {
                        case AnchorFace.XLow:
                            return LeftSliver(_units);
                        case AnchorFace.XHigh:
                            return RightSliver(_units);
                        case AnchorFace.YLow:
                            return BottomSliver(_units);
                        case AnchorFace.YHigh:
                            return TopSliver(_units);
                    }
                    return 0;
            }
        }
        #endregion

        #region public static ulong GetDiagonal(TriangleCorner corner)
        /// <summary>
        /// Complete diagonals (panels?) and stair simplification
        /// </summary>
        private static ulong GetDiagonal(TriangleCorner corner)
        {
            // TODO: merge with slope...
            switch (corner)
            {
                case TriangleCorner.LowerLeft:
                    return 0b10000000_11000000_11100000_11110000_11111000_11111100_11111110_11111111;

                case TriangleCorner.UpperLeft:
                    return 0b11111111_11111110_11111100_11111000_11110000_11100000_11000000_10000000;

                case TriangleCorner.LowerRight:
                    return 0b00000001_00000011_00000111_00001111_00011111_00111111_01111111_11111111;

                case TriangleCorner.UpperRight:
                default:
                    return 0b11111111_01111111_00111111_00011111_00001111_00000111_00000011_00000001;
            }
        }
        #endregion

        #region public static ulong GetSlope(Axis moveAxis, AnchorFace blockSnap, double loOff, double hiOff)
        private static readonly ulong[] _LowerRightSlopeTall =
        {
            0,
            0b00000000_00000000_00000000_00000000_00000001_00000001_00000001_00000001,
            0b00000000_00000000_00000001_00000001_00000001_00000001_00000011_00000011,
            0b00000000_00000000_00000001_00000001_00000011_00000011_00000111_00000111,
            0b00000000_00000001_00000001_00000011_00000011_00000111_00000111_00001111,
            0b00000000_00000001_00000011_00000011_00000111_00001111_00001111_00011111,
            0b00000000_00000001_00000011_00000111_00001111_00001111_00011111_00111111,
            0b00000000_00000001_00000011_00000111_00001111_00011111_00111111_01111111,
            0b00000001_00000011_00000111_00001111_00011111_00111111_01111111_11111111
        };

        private static readonly ulong[] _UpperRightSlopeTall =
        {
            0,
            0b00000001_00000001_00000001_00000001_00000000_00000000_00000000_00000000,
            0b00000011_00000011_00000001_00000001_00000001_00000001_00000000_00000000,
            0b00000111_00000111_00000011_00000011_00000001_00000001_00000000_00000000,
            0b00001111_00000111_00000111_00000011_00000011_00000001_00000001_00000000,
            0b00011111_00001111_00001111_00000111_00000011_00000011_00000001_00000000,
            0b00111111_00011111_00001111_00001111_00000111_00000011_00000001_00000000,
            0b01111111_00111111_00011111_00001111_00000111_00000011_00000001_00000000,
            0b11111111_01111111_00111111_00011111_00001111_00000111_00000011_00000001
        };

        private static readonly ulong[] _LowerLeftSlopeTall =
        {
            0,
            0b00000000_00000000_00000000_00000000_10000000_10000000_10000000_10000000,
            0b00000000_00000000_10000000_10000000_10000000_10000000_11000000_11000000,
            0b00000000_00000000_10000000_10000000_11000000_11000000_11100000_11100000,
            0b00000000_10000000_10000000_11000000_11000000_11100000_11100000_11110000,
            0b00000000_10000000_11000000_11000000_11100000_11110000_11110000_11111000,
            0b00000000_10000000_11000000_11100000_11110000_11110000_11111000_11111100,
            0b00000000_10000000_11000000_11100000_11110000_11111000_11111100_11111110,
            0b10000000_11000000_11100000_11110000_11111000_11111100_11111110_11111111
        };

        private static readonly ulong[] _UpperLeftSlopeTall =
        {
            0,
            0b10000000_10000000_10000000_10000000_00000000_00000000_00000000_00000000,
            0b11000000_11000000_10000000_10000000_10000000_10000000_00000000_00000000,
            0b11100000_11100000_11000000_11000000_10000000_10000000_00000000_00000000,
            0b11110000_11100000_11100000_11000000_11000000_10000000_10000000_00000000,
            0b11111000_11110000_11110000_11100000_11000000_11000000_10000000_00000000,
            0b11111100_11111000_11110000_11110000_11100000_11000000_10000000_00000000,
            0b11111110_11111100_11111000_11110000_11100000_11000000_10000000_00000000,
            0b11111111_11111110_11111100_11111000_11110000_11100000_11000000_10000000
        };

        private static readonly ulong[] _LowerRightSlopeWide =
        {
            0,
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001111,
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000011_00111111,
            0b00000000_00000000_00000000_00000000_00000000_00000011_00001111_00111111,
            0b00000000_00000000_00000000_00000000_00000001_00000111_00011111_01111111,
            0b00000000_00000000_00000000_00000001_00000111_00001111_00111111_01111111,
            0b00000000_00000000_00000001_00000111_00001111_00011111_00111111_01111111,
            0b00000000_00000001_00000011_00000111_00001111_00011111_00111111_01111111,
            0b00000001_00000011_00000111_00001111_00011111_00111111_01111111_11111111
        };

        private static readonly ulong[] _UpperRightSlopeWide =
        {
            0,
            0b00001111_00000000_00000000_00000000_00000000_00000000_00000000_00000000,
            0b00111111_00000011_00000000_00000000_00000000_00000000_00000000_00000000,
            0b00111111_00001111_00000011_00000000_00000000_00000000_00000000_00000000,
            0b01111111_00011111_00000111_00000001_00000000_00000000_00000000_00000000,
            0b01111111_00111111_00001111_00000111_00000001_00000000_00000000_00000000,
            0b01111111_00111111_00011111_00001111_00000111_00000001_00000000_00000000,
            0b01111111_00111111_00011111_00001111_00000111_00000011_00000001_00000000,
            0b11111111_01111111_00111111_00011111_00001111_00000111_00000011_00000001
        };

        private static readonly ulong[] _LowerLeftSlopeWide =
        {
            0,
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11110000,
            0b00000000_00000000_00000000_00000000_00000000_00000000_11000000_11111100,
            0b00000000_00000000_00000000_00000000_00000000_11000000_11110000_11111100,
            0b00000000_00000000_00000000_00000000_10000000_11100000_11111000_11111110,
            0b00000000_00000000_00000000_10000000_11100000_11110000_11111100_11111110,
            0b00000000_00000000_10000000_11100000_11110000_11111000_11111100_11111110,
            0b00000000_10000000_11000000_11100000_11110000_11111000_11111100_11111110,
            0b10000000_11000000_11100000_11110000_11111000_11111100_11111110_11111111
        };

        private static readonly ulong[] _UpperLeftSlopeWide =
        {
            0,
            0b11110000_00000000_00000000_00000000_00000000_00000000_00000000_00000000,
            0b11111100_11000000_00000000_00000000_00000000_00000000_00000000_00000000,
            0b11111100_11110000_11000000_00000000_00000000_00000000_00000000_00000000,
            0b11111110_11111000_11100000_10000000_00000000_00000000_00000000_00000000,
            0b11111110_11111100_11110000_11100000_10000000_00000000_00000000_00000000,
            0b11111110_11111100_11111000_11110000_11100000_10000000_00000000_00000000,
            0b11111110_11111100_11111000_11110000_11100000_11000000_10000000_00000000,
            0b11111111_11111110_11111100_11111000_11110000_11100000_11000000_10000000
        };

        private static ulong GetSlope(Axis moveAxis, AnchorFace blockSnap, double loOff, double hiOff)
        {
            var _loUnits = Math.Min((byte)Math.Round(loOff * 8 / 5), (byte)8);
            var _hiUnits = Math.Min((byte)Math.Round(hiOff * 8 / 5), (byte)8);
            var _idx = Math.Abs(_hiUnits - _loUnits);
            switch (moveAxis)
            {
                case Axis.X:
                    switch (blockSnap)
                    {
                        case AnchorFace.YLow:
                            // left tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerLeftSlopeTall[_idx] >> _hiUnits) | LeftSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperLeftSlopeTall[_idx] >> _loUnits) | LeftSliver(_loUnits);
                            }

                        case AnchorFace.YHigh:
                            // right tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerRightSlopeTall[_idx] << _hiUnits) | RightSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperRightSlopeTall[_idx] << _loUnits) | RightSliver(_loUnits);
                            }

                        case AnchorFace.ZLow:
                            // lower wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_LowerLeftSlopeWide[_idx] << (_hiUnits * 8)) | BottomSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_LowerRightSlopeWide[_idx] << (_loUnits * 8)) | BottomSliver(_loUnits);
                            }

                        case AnchorFace.ZHigh:
                            // upper wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_UpperLeftSlopeWide[_idx] >> (_hiUnits * 8)) | TopSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_UpperRightSlopeWide[_idx] >> (_loUnits * 8)) | TopSliver(_loUnits);
                            }
                    }
                    return 0;

                case Axis.Y:
                    switch (blockSnap)
                    {
                        case AnchorFace.XLow:
                            // lower wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_LowerLeftSlopeWide[_idx] << (_hiUnits * 8)) | BottomSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_LowerRightSlopeWide[_idx] << (_loUnits * 8)) | BottomSliver(_loUnits);
                            }

                        case AnchorFace.XHigh:
                            // upper wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_UpperLeftSlopeWide[_idx] >> (_hiUnits * 8)) | TopSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_UpperRightSlopeWide[_idx] >> (_loUnits * 8)) | TopSliver(_loUnits);
                            }

                        case AnchorFace.ZLow:
                            // left tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerLeftSlopeTall[_idx] >> _hiUnits) | LeftSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperLeftSlopeTall[_idx] >> _loUnits) | LeftSliver(_loUnits);
                            }

                        case AnchorFace.ZHigh:
                            // right tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerRightSlopeTall[_idx] << _hiUnits) | RightSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperRightSlopeTall[_idx] << _loUnits) | RightSliver(_loUnits);
                            }
                    }
                    return 0;

                case Axis.Z:
                default:
                    switch (blockSnap)
                    {
                        case AnchorFace.XLow:
                            // left tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerLeftSlopeTall[_idx] >> _hiUnits) | LeftSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperLeftSlopeTall[_idx] >> _loUnits) | LeftSliver(_loUnits);
                            }

                        case AnchorFace.XHigh:
                            // right tall
                            if (_loUnits > _hiUnits)
                            {
                                // lower
                                return (_LowerRightSlopeTall[_idx] << _hiUnits) | RightSliver(_hiUnits);
                            }
                            else
                            {
                                // upper
                                return (_UpperRightSlopeTall[_idx] << _loUnits) | RightSliver(_loUnits);
                            }

                        case AnchorFace.YLow:
                            // lower wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_LowerLeftSlopeWide[_idx] << (_hiUnits * 8)) | BottomSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_LowerRightSlopeWide[_idx] << (_loUnits * 8)) | BottomSliver(_loUnits);
                            }

                        case AnchorFace.YHigh:
                            // upper wide
                            if (_loUnits > _hiUnits)
                            {
                                // left
                                return (_UpperLeftSlopeWide[_idx] >> (_hiUnits * 8)) | TopSliver(_hiUnits);
                            }
                            else
                            {
                                // right
                                return (_UpperRightSlopeWide[_idx] >> (_loUnits * 8)) | TopSliver(_loUnits);
                            }
                    }
                    return 0;
            }
        }

        private static ulong GetWedge(double primeOff, double secondOff, bool flip)
        {
            // magnitude of offset
            var _pMag = Math.Abs(primeOff);
            var _sMag = Math.Abs(secondOff);

            // units for offset
            var _pUnits = Math.Min((byte)Math.Round(_pMag * 8 / 5), (byte)8);
            var _sUnits = Math.Min((byte)Math.Round(_sMag * 8 / 5), (byte)8);

            // corner flags
            var _wide = _pMag > _sMag; // else tall
            var _upper = (secondOff < 0) ^ flip;
            var _right = (primeOff < 0) ^ flip;

            // scale smaller dimension by same factor as larger dimension
            // then project into a scale from 0 to 8 (integerized units)
            var _idx = (byte)Math.Round(((1 / (_wide ? _pMag : _sMag)) * (_wide ? _sMag : _pMag)) * 8);
            var _grip = _wide
                ? (_upper
                ? (_right ? _UpperRightSlopeWide[_idx] : _UpperLeftSlopeWide[_idx])
                : (_right ? _LowerRightSlopeWide[_idx] : _LowerLeftSlopeWide[_idx]))
                : (_upper
                ? (_right ? _UpperRightSlopeTall[_idx] : _UpperLeftSlopeTall[_idx])
                : (_right ? _LowerRightSlopeTall[_idx] : _LowerLeftSlopeTall[_idx]));
            if (flip)
            {
                if (_wide)
                {
                    #region flipped wide
                    var _horz = (byte)(8 - _pUnits);
                    var _vert = (byte)(8 - _idx);
                    if (_upper)
                    {
                        // shift down
                        _grip >>= (_vert * 8);
                        if (_right)
                        {
                            // upper right wide flipped
                            _grip <<= _horz;
                            _grip |= RightSliver(_horz);
                        }
                        else
                        {
                            // upper left wide flipped
                            _grip >>= _horz;
                            _grip |= LeftSliver(_horz);
                        }
                        _grip |= TopSliver(_vert);
                    }
                    else
                    {
                        // lower: shift up
                        _grip <<= (_vert * 8);
                        if (_right)
                        {
                            // lower right wide flipped
                            _grip <<= _horz;
                            _grip |= RightSliver(_horz);
                        }
                        else
                        {
                            // lower left wide flipped
                            _grip >>= _horz;
                            _grip |= LeftSliver(_horz);
                        }
                        _grip |= BottomSliver(_vert);
                    }
                    #endregion
                }
                else
                {
                    #region flipped narrow
                    // narrow
                    var _horz = (byte)(8 - _idx);
                    var _vert = (byte)((8 - _sUnits) * 8);
                    if (_upper)
                    {
                        // shift down
                        _grip >>= (_vert * 8);
                        if (_right)
                        {
                            // upper right narrow flipped
                            _grip <<= _horz;
                            _grip |= RightSliver(_horz);
                        }
                        else
                        {
                            // upper left narrow flipped
                            _grip >>= _horz;
                            _grip |= LeftSliver(_horz);
                        }
                        _grip |= TopSliver(_vert);
                    }
                    else
                    {
                        // lower: shift up
                        _grip <<= (_vert * 8);
                        if (_right)
                        {
                            // lower right wide flipped
                            _grip <<= _horz;
                            _grip |= RightSliver(_horz);
                        }
                        else
                        {
                            // lower left wide flipped
                            _grip >>= _horz;
                            _grip |= LeftSliver(_horz);
                        }
                        _grip |= BottomSliver(_vert);
                    }
                    #endregion
                }
                return _grip;
            }
            else
            {
                #region unflipped
                if (_wide)
                {
                    // since we went wide, only have to shift left or right
                    var _horz = (byte)(8 - _pUnits);
                    if (_right)
                    {
                        return (_grip >> _horz);
                    }

                    // left
                    return _grip << _horz;
                }
                else
                {
                    // since we went tall, only have to shift up or down
                    var _vert = (byte)((8 - _sUnits) * 8);
                    if (_upper)
                    {
                        return _grip << _vert;
                    }

                    // lower
                    return _grip >> _vert;
                }
                #endregion
            }
        }
        #endregion

        // TODO: non-complete diagonal bit fills...
        // TODO: "auto"-combining (corner)

        private static ulong GetCircle()
            => 0b00111100_01111110_11111111_11111111_11111111_11111111_01111110_00111100;

        private static readonly ulong _QuarterCircle = 0b00000110_00001111_00001111_00000110;
        private static ulong GetQuarterCircle(TriangleCorner corner)
        {
            switch (corner)
            {
                case TriangleCorner.UpperLeft:
                    return _QuarterCircle << 36;
                case TriangleCorner.UpperRight:
                    return _QuarterCircle << 32;
                case TriangleCorner.LowerLeft:
                    return _QuarterCircle << 4;
                case TriangleCorner.LowerRight:
                default:
                    return _QuarterCircle;
            }
        }
    }
}
