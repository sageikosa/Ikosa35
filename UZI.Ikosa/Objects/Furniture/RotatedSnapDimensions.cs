using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Objects
{
    public readonly struct RotatedSnapDimensions
    {
        #region ctor()
        public RotatedSnapDimensions(double chord, double vector)
        {
            _Chord = chord;
            _Vector = vector;
            _DSnap = 0d;
            _SSnap = 0d;
            _NSnap = 0d;
            // lambda struct capture
            var _vector = Vector;
            var _chord = Chord;

            // nape
            var _farPt = (Chord / 2d + Vector);
            var _useNape = _farPt < DIAGONAL;
            double _nape() => _useNape ? Math.Pow(DIAGONAL - _vector - (_chord / 2d), 2) : 0d;

            // corner
            double _corner() => Math.Pow(DIAGONAL - (_chord / 2d), 2d);

            // far corner
            var _hasFarPt = _farPt > DIAGONAL;
            double _farCorner() => Math.Pow(_farPt - DIAGONAL, 2d);

            // COVERAGES
            if (Chord < DIAGONAL)
            {
                // bits of snap-snap cell
                double _nose() => Math.Pow(_chord, 2d) / 4d;

                // dog-ears
                var _useDogEars = Chord < DIAGONAL;
                double _dogEars() => _useDogEars ? Math.Pow(5 - Math.Sqrt(_chord * _chord / 2d), 2d) : 0d;
                var _useFarCorner = Chord >= (_farPt - DIAGONAL) * 2d;

                _DSnap = 25d - (_nose() + _dogEars() + _nape()); // subtractive
                if (!_hasFarPt)
                {
                    _SSnap = 0.5d * (Chord * Vector - _DSnap);
                }
                else if (_useFarCorner)
                {
                    _NSnap = _farCorner();
                    _SSnap = 0.5d * (Chord * Vector - (_DSnap + _NSnap));
                }
                else
                {
                    _SSnap = Math.Pow(Chord / 2d, 2d);
                    _NSnap = Chord * Vector - (_DSnap + 2d * _SSnap);
                }
            }
            else if (Chord == DIAGONAL)
            {
                _DSnap = _corner() - _nape(); // additive
                if (Vector >= DIAGONAL)
                {
                    _SSnap = 12.5d;
                    _NSnap = 12.5d;
                }
                else if (Vector == Chord)
                {
                    _SSnap = 6.25d;
                }
                else if (_useNape)
                {
                    _SSnap = Vector * Vector / 2d;
                }
                else
                {
                    _NSnap = _farCorner();
                    _SSnap = 0.5d * (Chord * Vector - (_DSnap + _NSnap));
                }
            }
            else
            {
                _DSnap = _corner() - _nape(); // additive
                if (!_hasFarPt)
                {
                    _SSnap = 0.5 * (Chord * Vector - _DSnap);
                }
                else
                {
                    _NSnap = _farCorner();
                    _SSnap = 0.5 * (Chord * Vector - (_DSnap + _NSnap));
                }
            }
        }
        #endregion

        private static readonly double DIAGONAL = Math.Sqrt(50);

        #region data
        private readonly double _Chord;
        private readonly double _Vector;

        private readonly double _DSnap;
        private readonly double _SSnap;
        private readonly double _NSnap;
        #endregion

        public double Chord => _Chord;
        public double Vector => _Vector;

        public double DoubleSnap => _DSnap;
        public double SingleSnap => _SSnap;
        public double NoSnap => _NSnap;
    }
}
