using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LFrame : ComponentSpace, ILFrameSpace
    {
        public LFrame(CellMaterial material, TileSet tileSet, CellMaterial plusMaterial, TileSet plusTiles,
            double width1, double width2, double thickness)
            : base(material, tileSet, plusMaterial, plusTiles)
        {
            _Width1 = width1;
            _Width2 = width2;
            _Thickness = thickness;
        }

        #region private data
        private double _Width1;
        private double _Width2;
        private double _Thickness;
        #endregion

        #region public double Width1 { get; set; }
        public double Width1
        {
            get { return _Width1; }
            set
            {
                if (_Width1 != value)
                {
                    _Width1 = value;
                    DoPropertyChanged(@"Offset1");
                }
            }
        }
        #endregion

        #region public double Width2 { get; set; }
        public double Width2
        {
            get { return _Width2; }
            set
            {
                if (_Width2 != value)
                {
                    _Width2 = value;
                    DoPropertyChanged(@"Offset2");
                }
            }
        }
        #endregion

        #region public double Thickness { get; set; }
        public double Thickness
        {
            get { return _Thickness; }
            set
            {
                if (_Thickness != value)
                {
                    _Thickness = value;
                    DoPropertyChanged(@"Thickness");
                }
            }
        }
        #endregion

        public override string GetDescription(uint param)
            => $@"LFr:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName}) [{Thickness};{Width1};{Width2}]";

        public override string GetParamText(uint param)
        {
            var _thickFace = LFrameSpaceFaces.GetThickFace(param);
            var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
            var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
            return $@"Thick={_thickFace}, F1={_frame1Face}, F2={_frame2Face}";
        }

        #region protected override IEnumerable<CellStructure> Components(uint param)
        protected override IEnumerable<CellStructure> Components(uint param)
        {
            var _thickFace = LFrameSpaceFaces.GetThickFace(param);
            var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
            var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
            CornerCellSpace _frame1 = null;
            CornerCellSpace _frame2 = null;

            void _create(Axis axis)
            {
                if (_frame1Face.GetAxis() == axis)
                {
                    _frame1 = new CornerCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling, Width1, Thickness);
                    _frame2 = new CornerCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling, Thickness, Width2);
                }
                else
                {
                    _frame1 = new CornerCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling, Thickness, Width1);
                    _frame2 = new CornerCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling, Width2, Thickness);
                }
            };

            switch (_thickFace.GetAxis())
            {
                case Axis.X:
                    _create(Axis.Z);
                    break;

                case Axis.Y:
                    _create(Axis.X);
                    break;

                case Axis.Z:
                    _create(Axis.Y);
                    break;
            }

            yield return new CellStructure(
                _frame1,
                StairSpaceFaces.WedgeParallelParam(_thickFace.ReverseFace(), _frame1Face.ReverseFace()));

            yield return new CellStructure(
                _frame2,
                StairSpaceFaces.WedgeParallelParam(_thickFace.ReverseFace(), _frame2Face.ReverseFace()));
            yield break;
        }
        #endregion

        private IEnumerable<AnchorFace> Faces(params AnchorFace[] faces)
            => faces.AsEnumerable();

        #region public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                // nothing
            }
            else if (!_cellBlock && !_plusBlock)
            {
                foreach (var _pt in base.TacticalPoints(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                var _thickFace = LFrameSpaceFaces.GetThickFace(param);
                var _openFace = _thickFace.ReverseFace();
                var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
                var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
                var _frameFaces = Faces(_frame1Face, _frame2Face).ToList();

                // critical values
                var _lowZ = 0d;
                var _lowY = 0d;
                var _lowX = 0d;
                var _highZ = 5d;
                var _highY = 5d;
                var _highX = 5d;

                // declare functions
                Func<IEnumerable<AnchorFace>, bool> _outerCorner;
                Func<IEnumerable<AnchorFace>, bool> _regularEdge;

                double _midX;
                double _midY;
                double _midZ;
                double _zOff;
                double _yOff;
                double _xOff;

                #region coordinate single values
                double _zNotch = 0;
                double _zEdge = 0;
                double _zAntiEdge = 5;
                double _yNotch = 0;
                double _yEdge = 0;
                double _yAntiEdge = 5;
                double _xNotch = 0;
                double _xEdge = 0;
                double _xAntiEdge = 5;
                #endregion

                #region notch and edge coordinates
                if (_frameFaces.Contains(AnchorFace.ZLow))
                {
                    _zEdge = 5;
                    _zAntiEdge = 0;
                    if (_frame1Face == AnchorFace.ZLow)
                    {
                        _zNotch = Width1;
                    }
                    else
                    {
                        _zNotch = Width2;
                    }
                }
                else if (_frameFaces.Contains(AnchorFace.ZHigh))
                {
                    if (_frame1Face == AnchorFace.ZHigh)
                    {
                        _zNotch = 5 - Width1;
                    }
                    else
                    {
                        _zNotch = 5 - Width2;
                    }
                }
                if (_frameFaces.Contains(AnchorFace.YLow))
                {
                    _yEdge = 5;
                    _yAntiEdge = 0;
                    if (_frame1Face == AnchorFace.YLow)
                    {
                        _yNotch = Width1;
                    }
                    else
                    {
                        _yNotch = Width2;
                    }
                }
                else if (_frameFaces.Contains(AnchorFace.YHigh))
                {
                    if (_frame1Face == AnchorFace.YHigh)
                    {
                        _yNotch = 5 - Width1;
                    }
                    else
                    {
                        _yNotch = 5 - Width2;
                    }
                }
                if (_frameFaces.Contains(AnchorFace.XLow))
                {
                    _xEdge = 5;
                    _xAntiEdge = 0;
                    if (_frame1Face == AnchorFace.XLow)
                    {
                        _xNotch = Width1;
                    }
                    else
                    {
                        _xNotch = Width2;
                    }
                }
                else if (_frameFaces.Contains(AnchorFace.XHigh))
                {
                    if (_frame1Face == AnchorFace.XHigh)
                    {
                        _xNotch = 5 - Width1;
                    }
                    else
                    {
                        _xNotch = 5 - Width2;
                    }
                }
                #endregion

                if (_cellBlock)
                {
                    // solid "L"

                    #region adjust irregular edges
                    // adjust irregular edges
                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                            _lowZ = Thickness;
                            break;
                        case AnchorFace.ZHigh:
                            _highZ -= Thickness;
                            break;
                        case AnchorFace.YLow:
                            _lowY = Thickness;
                            break;
                        case AnchorFace.YHigh:
                            _highY -= Thickness;
                            break;
                        case AnchorFace.XLow:
                            _lowX = Thickness;
                            break;
                        case AnchorFace.XHigh:
                            _highX -= Thickness;
                            break;
                    }
                    #endregion

                    #region midpoints and offsets for irregular edges
                    _midX = (_lowX + _highX) / 2;
                    _midY = (_lowY + _highY) / 2;
                    _midZ = (_lowZ + _highZ) / 2;
                    _zOff = _highZ - _lowZ;
                    _yOff = _highY - _lowY;
                    _xOff = _highX - _lowX;
                    #endregion

                    _outerCorner = (faces) =>
                    {
                        // non thickFace corners have openCorners (which are good) ...
                        // ... the only thickCorner that is good has no frameFaces
                        return !faces.Contains(_thickFace) || !faces.Intersect(_frameFaces).Any();
                    };

                    _regularEdge = (faces) =>
                    {
                        // open face is good...
                        // ... if not on a thickface, then only good if it also has no framefaces
                        return faces.Contains(_openFace) || (!faces.Contains(_thickFace) && !faces.Intersect(_frameFaces).Any());
                    };

                    #region regular outer face
                    // NOTE: only openFace is an outer regular face
                    if (_openFace == AnchorFace.ZLow)
                    {
                        yield return new Point3D(2.5, 2.5, 0);
                    }

                    if (_openFace == AnchorFace.ZHigh)
                    {
                        yield return new Point3D(2.5, 2.5, 5);
                    }

                    if (_openFace == AnchorFace.YLow)
                    {
                        yield return new Point3D(2.5, 0, 2.5);
                    }

                    if (_openFace == AnchorFace.YHigh)
                    {
                        yield return new Point3D(2.5, 5, 2.5);
                    }

                    if (_openFace == AnchorFace.XLow)
                    {
                        yield return new Point3D(0, 2.5, 2.5);
                    }

                    if (_openFace == AnchorFace.XHigh)
                    {
                        yield return new Point3D(5, 2.5, 2.5);
                    }
                    #endregion

                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            {
                                #region points for thick Z
                                var _outer = (_thickFace == AnchorFace.ZLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.ZLow) ? _lowZ : _highZ;

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_xEdge, _yNotch, _outer);
                                yield return new Point3D(_xNotch, _yEdge, _outer);
                                yield return new Point3D(_xNotch, _yNotch, _outer);

                                // 3 pts @ inner with FrameOffsets
                                yield return new Point3D(_xEdge, _yNotch, _inner);
                                yield return new Point3D(_xNotch, _yEdge, _inner);
                                yield return new Point3D(_xNotch, _yNotch, _inner);

                                // 3 pts @ inner at frameFace edges
                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, _thickFace)))
                                {
                                    yield return new Point3D(0, 0, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, _thickFace)))
                                {
                                    yield return new Point3D(5, 0, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, _thickFace)))
                                {
                                    yield return new Point3D(0, 5, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, _thickFace)))
                                {
                                    yield return new Point3D(5, 5, _inner);
                                }

                                // 3 edges of XNotch
                                if (Math.Abs(_xNotch - _xEdge) >= 2.5)
                                {
                                    var _midPt = (_xNotch + _xEdge) / 2;
                                    yield return new Point3D(_midPt, _yEdge, _outer);
                                    yield return new Point3D(_midPt, _yNotch, _outer);
                                    yield return new Point3D(_midPt, _yNotch, _inner);
                                    if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                    {
                                        // outer ZFace
                                        yield return new Point3D(_midPt, (_yNotch + _yEdge) / 2, _outer);
                                    }

                                    // irregular yEdge face mid-point
                                    yield return new Point3D(_midPt, _yEdge, 2.5);
                                }

                                // 3 edges of YNotch
                                if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                {
                                    var _midPt = (_yNotch + _yEdge) / 2;
                                    yield return new Point3D(_xEdge, _midPt, _outer);
                                    yield return new Point3D(_xNotch, _midPt, _outer);
                                    yield return new Point3D(_xNotch, _midPt, _inner);

                                    // irregular xEdge face Mid-Point
                                    yield return new Point3D(_xEdge, _midPt, 2.5);
                                }

                                // 2 edges from Z=outer to Z=inner at revFrameFace
                                if (Thickness >= 2.5)
                                {
                                    yield return new Point3D(_xNotch, _yEdge, (_inner + _outer) / 2);
                                    yield return new Point3D(_xEdge, _yNotch, (_inner + _outer) / 2);
                                }

                                // 2 edges @ inner (reverse FrameFace, FrameOffset)
                                if (Math.Abs(_xNotch - _xEdge) <= 2.5)
                                {
                                    yield return new Point3D((_xNotch + _xAntiEdge) / 2, _yEdge, _inner);
                                }

                                if (Math.Abs(_yNotch - _yEdge) <= 2.5)
                                {
                                    yield return new Point3D(_xEdge, (_yNotch + _yAntiEdge) / 2, _inner);
                                }

                                // 2 edges @ inner (FrameFace)
                                if (_frameFaces.Contains(AnchorFace.XLow))
                                {
                                    yield return new Point3D(0, 2.5, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.XHigh))
                                {
                                    yield return new Point3D(5, 2.5, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.YLow))
                                {
                                    yield return new Point3D(2.5, 0, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.YHigh))
                                {
                                    yield return new Point3D(2.5, 5, _inner);
                                }

                                // 4 irregular side face points
                                if (_zOff >= 2.5)
                                {
                                    yield return new Point3D(0, 2.5, _midZ);
                                    yield return new Point3D(5, 2.5, _midZ);
                                    yield return new Point3D(2.5, 0, _midZ);
                                    yield return new Point3D(2.5, 5, _midZ);
                                }
                                #endregion
                            }
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            {
                                #region points for thick Y
                                var _outer = (_thickFace == AnchorFace.YLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.YLow) ? _lowY : _highY;

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_xEdge, _outer, _zNotch);
                                yield return new Point3D(_xNotch, _outer, _zEdge);
                                yield return new Point3D(_xNotch, _outer, _zNotch);

                                // 3 pts @ inner with FrameOffsets
                                yield return new Point3D(_xEdge, _inner, _zNotch);
                                yield return new Point3D(_xNotch, _inner, _zEdge);
                                yield return new Point3D(_xNotch, _inner, _zNotch);

                                // 3 pts @ lowZ at frameFace edges
                                if (!_outerCorner(Faces(AnchorFace.XLow, _thickFace, AnchorFace.ZLow)))
                                {
                                    yield return new Point3D(0, _inner, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, _thickFace, AnchorFace.ZLow)))
                                {
                                    yield return new Point3D(5, _inner, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XLow, _thickFace, AnchorFace.ZHigh)))
                                {
                                    yield return new Point3D(0, _inner, 5);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, _thickFace, AnchorFace.ZHigh)))
                                {
                                    yield return new Point3D(5, _inner, 5);
                                }

                                // 3 edges of ZNotch
                                if (Math.Abs(_zNotch - _zEdge) >= 2.5)
                                {
                                    var _midPt = (_zNotch + _zEdge) / 2;
                                    yield return new Point3D(_xEdge, _outer, _midPt);
                                    yield return new Point3D(_xNotch, _outer, _midPt);
                                    yield return new Point3D(_xNotch, _inner, _midPt);
                                    if (Math.Abs(_xNotch - _xEdge) >= 2.5)
                                    {
                                        // outer YFace
                                        yield return new Point3D((_xNotch + _xEdge) / 2, _outer, _midPt);
                                    }

                                    // irregular xEdge face mid-point
                                    yield return new Point3D(_xEdge, 2.5, _midPt);
                                }

                                // 3 edges of xNotch
                                if (Math.Abs(_xNotch - _xEdge) >= 2.5)
                                {
                                    var _midPt = (_xNotch + _xEdge) / 2;
                                    yield return new Point3D(_midPt, _outer, _zEdge);
                                    yield return new Point3D(_midPt, _outer, _zNotch);
                                    yield return new Point3D(_midPt, _inner, _zNotch);

                                    // irregular zEdge face Mid-Point
                                    yield return new Point3D(_midPt, 2.5, _zEdge);
                                }

                                // 2 edges from Y=outer to Y=inner at revFrameFace
                                if (Thickness >= 2.5)
                                {
                                    yield return new Point3D(_xNotch, (_inner + _outer) / 2, _zEdge);
                                    yield return new Point3D(_xEdge, (_inner + _outer) / 2, _zNotch);
                                }

                                // 2 edges @ inner (reverse FrameFace, FrameOffset)
                                if (Math.Abs(_xNotch - _xEdge) <= 2.5)
                                {
                                    yield return new Point3D((_xNotch + _xAntiEdge) / 2, _inner, _zEdge);
                                }

                                if (Math.Abs(_zNotch - _zEdge) <= 2.5)
                                {
                                    yield return new Point3D(_xEdge, _inner, (_zNotch + _zAntiEdge) / 2);
                                }

                                // 2 edges @ inner (FrameFace)
                                if (_frameFaces.Contains(AnchorFace.XLow))
                                {
                                    yield return new Point3D(0, _inner, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.XHigh))
                                {
                                    yield return new Point3D(5, _inner, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZLow))
                                {
                                    yield return new Point3D(2.5, _inner, 0);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZHigh))
                                {
                                    yield return new Point3D(2.5, _inner, 5);
                                }

                                // 4 irregular side face points
                                if (_yOff >= 2.5)
                                {
                                    yield return new Point3D(0, _midY, 2.5);
                                    yield return new Point3D(5, _midY, 2.5);
                                    yield return new Point3D(2.5, _midY, 0);
                                    yield return new Point3D(2.5, _midY, 5);
                                }
                                #endregion
                            }
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            {
                                #region points for thick X
                                var _outer = (_thickFace == AnchorFace.XLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.XLow) ? _lowX : _highX;

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_outer, _yEdge, _zNotch);
                                yield return new Point3D(_outer, _yNotch, _zEdge);
                                yield return new Point3D(_outer, _yNotch, _zNotch);

                                // 3 pts @ inner with FrameOffsets
                                yield return new Point3D(_inner, _yEdge, _zNotch);
                                yield return new Point3D(_inner, _yNotch, _zEdge);
                                yield return new Point3D(_inner, _yNotch, _zNotch);

                                // 3 pts @ inner at frameFace edges
                                if (!_outerCorner(Faces(_thickFace, AnchorFace.YLow, AnchorFace.ZLow)))
                                {
                                    yield return new Point3D(_inner, 0, 0);
                                }

                                if (!_outerCorner(Faces(_thickFace, AnchorFace.YHigh, AnchorFace.ZLow)))
                                {
                                    yield return new Point3D(_inner, 5, 0);
                                }

                                if (!_outerCorner(Faces(_thickFace, AnchorFace.YLow, AnchorFace.ZHigh)))
                                {
                                    yield return new Point3D(_inner, 0, 5);
                                }

                                if (!_outerCorner(Faces(_thickFace, AnchorFace.YHigh, AnchorFace.ZHigh)))
                                {
                                    yield return new Point3D(_inner, 5, 5);
                                }

                                // 3 edges of ZNotch
                                if (Math.Abs(_zNotch - _zEdge) >= 2.5)
                                {
                                    var _midPt = (_zNotch + _zEdge) / 2;
                                    yield return new Point3D(_outer, _yEdge, _midPt);
                                    yield return new Point3D(_outer, _yNotch, _midPt);
                                    yield return new Point3D(_inner, _yNotch, _midPt);
                                    if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                    {
                                        // outer XFace
                                        yield return new Point3D(_outer, (_yNotch + _yEdge) / 2, _midPt);
                                    }

                                    // irregular yEdge face mid-point
                                    yield return new Point3D(2.5, _yEdge, _midPt);
                                }

                                // 3 edges of yNotch
                                if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                {
                                    var _midPt = (_yNotch + _yEdge) / 2;
                                    yield return new Point3D(_outer, _midPt, _zEdge);
                                    yield return new Point3D(_outer, _midPt, _zNotch);
                                    yield return new Point3D(_inner, _midPt, _zNotch);

                                    // irregular zEdge face Mid-Point
                                    yield return new Point3D(2.5, _midPt, _zEdge);
                                }

                                // 2 edges from X=outer to X=inner at revFrameFace
                                if (Thickness >= 2.5)
                                {
                                    yield return new Point3D((_inner + _outer) / 2, _yNotch, _zEdge);
                                    yield return new Point3D((_inner + _outer) / 2, _yEdge, _zNotch);
                                }

                                // 2 edges @ inner (reverse FrameFace, FrameOffset)
                                if (Math.Abs(_yNotch - _yEdge) <= 2.5)
                                {
                                    yield return new Point3D(_inner, (_yNotch + _yAntiEdge) / 2, _zEdge);
                                }

                                if (Math.Abs(_zNotch - _zEdge) <= 2.5)
                                {
                                    yield return new Point3D(_inner, _yEdge, (_zNotch + _zAntiEdge) / 2);
                                }

                                // 2 edges @ inner (FrameFace)
                                if (_frameFaces.Contains(AnchorFace.YLow))
                                {
                                    yield return new Point3D(_inner, 0, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.YHigh))
                                {
                                    yield return new Point3D(_inner, 5, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZLow))
                                {
                                    yield return new Point3D(_inner, 2.5, 0);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZHigh))
                                {
                                    yield return new Point3D(_inner, 2.5, 5);
                                }

                                // 4 irregular side face points
                                if (_xOff >= 2.5)
                                {
                                    yield return new Point3D(_midX, 0, 2.5);
                                    yield return new Point3D(_midX, 5, 2.5);
                                    yield return new Point3D(_midX, 2.5, 0);
                                    yield return new Point3D(_midX, 2.5, 5);
                                }
                                #endregion
                            }
                            break;
                    }
                }
                else
                {
                    // hollow "L"

                    #region irregular edges
                    // adjust irregular edges
                    switch (_thickFace)
                    {
                        case AnchorFace.ZHigh:
                            _lowZ = 5 - Thickness;
                            break;
                        case AnchorFace.ZLow:
                            _highZ = Thickness;
                            break;
                        case AnchorFace.YHigh:
                            _lowY = 5 - Thickness;
                            break;
                        case AnchorFace.YLow:
                            _highY = Thickness;
                            break;
                        case AnchorFace.XHigh:
                            _lowX = 5 - Thickness;
                            break;
                        case AnchorFace.XLow:
                            _highX = Thickness;
                            break;
                    }
                    #endregion

                    #region midpoints and offsets for irregular edges
                    _midX = (_lowX + _highX) / 2;
                    _midY = (_lowY + _highY) / 2;
                    _midZ = (_lowZ + _highZ) / 2;
                    _zOff = _highZ - _lowZ;
                    _yOff = _highY - _lowY;
                    _xOff = _highX - _lowX;
                    #endregion

                    _outerCorner = (faces) =>
                    {
                        // regular outer corners have thickface and any frameface
                        return faces.Contains(_thickFace) && faces.Intersect(_frameFaces).Any();
                    };

                    _regularEdge = (faces) =>
                    {
                        // regular outer edges have thickface and a frameface
                        return faces.Contains(_thickFace) && faces.Intersect(_frameFaces).Any();
                    };

                    // NOTE: no regular outer faces!

                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            {
                                #region points for thick Z
                                var _outer = (_thickFace == AnchorFace.ZLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.ZLow) ? _highZ : _lowZ;

                                // 3 pts @ inner at frameFace edges
                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, _thickFace)))
                                {
                                    yield return new Point3D(0, 0, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, _thickFace)))
                                {
                                    yield return new Point3D(5, 0, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, _thickFace)))
                                {
                                    yield return new Point3D(0, 5, _inner);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, _thickFace)))
                                {
                                    yield return new Point3D(5, 5, _inner);
                                }

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_xEdge, _yNotch, _outer);
                                yield return new Point3D(_xNotch, _yEdge, _outer);
                                yield return new Point3D(_xNotch, _yNotch, _outer);

                                // 2 pts @ inner with FrameOffsets
                                yield return new Point3D(_xEdge, _yNotch, _inner);
                                yield return new Point3D(_xNotch, _yEdge, _inner);

                                // 2 edges @ inner (on frameface)
                                if (_frameFaces.Contains(AnchorFace.XLow))
                                {
                                    yield return new Point3D(0, 2.5, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.XHigh))
                                {
                                    yield return new Point3D(5, 2.5, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.YLow))
                                {
                                    yield return new Point3D(2.5, 0, _inner);
                                }

                                if (_frameFaces.Contains(AnchorFace.YHigh))
                                {
                                    yield return new Point3D(2.5, 5, _inner);
                                }

                                if (_zOff >= 2.5)
                                {
                                    // 2 faces on frameface
                                    if (_frameFaces.Contains(AnchorFace.XLow))
                                    {
                                        yield return new Point3D(0, 2.5, _midZ);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.XHigh))
                                    {
                                        yield return new Point3D(5, 2.5, _midZ);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.YLow))
                                    {
                                        yield return new Point3D(2.5, 0, _midZ);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.YHigh))
                                    {
                                        yield return new Point3D(2.5, 5, _midZ);
                                    }

                                    // xNotch edge point
                                    yield return new Point3D(_xNotch, _yEdge, _midZ);

                                    // yNotch edge point
                                    yield return new Point3D(_xEdge, _yNotch, _midZ);
                                }

                                // mid xNotch (1) @ Outer
                                if (Math.Abs(_xNotch - _xEdge) >= 2.5)
                                {
                                    yield return new Point3D((_xNotch + _xEdge) / 2, _yNotch, _outer);
                                }

                                // mid yNotch (1) @ Outer
                                if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                {
                                    yield return new Point3D(_xNotch, (_yNotch + _yEdge) / 2, _outer);
                                }

                                // mid anti-xNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_xNotch - _xEdge) <= 2.5)
                                {
                                    var _midPt = (_xNotch + _xAntiEdge) / 2;
                                    yield return new Point3D(_midPt, _yEdge, _inner);
                                    yield return new Point3D(_midPt, _yEdge, _outer);
                                    yield return new Point3D(_midPt, _yEdge, _midZ);

                                    // irregular outer face
                                    yield return new Point3D(_midPt, 2.5, _outer);
                                }

                                // mid anti-yNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_yNotch - _yEdge) <= 2.5)
                                {
                                    var _midPt = (_yNotch + _yAntiEdge) / 2;
                                    yield return new Point3D(_xEdge, _midPt, _inner);
                                    yield return new Point3D(_xEdge, _midPt, _outer);
                                    yield return new Point3D(_xEdge, _midPt, _midZ);

                                    // irregular outer face
                                    yield return new Point3D(2.5, _midPt, _outer);
                                }
                                #endregion
                            }
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            {
                                #region points for thick Y
                                var _outer = (_thickFace == AnchorFace.YLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.YLow) ? _highY : _lowY;

                                // 3 pts @ inner at frameFace edges
                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.ZLow, _thickFace)))
                                {
                                    yield return new Point3D(0, _inner, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.ZLow, _thickFace)))
                                {
                                    yield return new Point3D(5, _inner, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.ZHigh, _thickFace)))
                                {
                                    yield return new Point3D(0, _inner, 5);
                                }

                                if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.ZHigh, _thickFace)))
                                {
                                    yield return new Point3D(5, _inner, 5);
                                }

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_xEdge, _outer, _zNotch);
                                yield return new Point3D(_xNotch, _outer, _zEdge);
                                yield return new Point3D(_xNotch, _outer, _zNotch);

                                // 2 pts @ inner with FrameOffsets
                                yield return new Point3D(_xEdge, _inner, _zNotch);
                                yield return new Point3D(_xNotch, _inner, _zEdge);

                                // 2 edges @ inner (on frameface)
                                if (_frameFaces.Contains(AnchorFace.XLow))
                                {
                                    yield return new Point3D(0, _inner, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.XHigh))
                                {
                                    yield return new Point3D(5, _inner, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZLow))
                                {
                                    yield return new Point3D(2.5, _inner, 0);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZHigh))
                                {
                                    yield return new Point3D(2.5, _inner, 5);
                                }

                                if (_yOff >= 2.5)
                                {
                                    // 2 faces on frameface
                                    if (_frameFaces.Contains(AnchorFace.XLow))
                                    {
                                        yield return new Point3D(0, _midY, 2.5);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.XHigh))
                                    {
                                        yield return new Point3D(5, _midY, 2.5);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.ZLow))
                                    {
                                        yield return new Point3D(2.5, _midY, 0);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.ZHigh))
                                    {
                                        yield return new Point3D(2.5, _midY, 5);
                                    }

                                    // xNotch edge point
                                    yield return new Point3D(_xNotch, _midY, _zEdge);

                                    // zNotch edge point
                                    yield return new Point3D(_xEdge, _midY, _zNotch);
                                }

                                // mid xNotch (1) @ Outer
                                if (Math.Abs(_xNotch - _xEdge) >= 2.5)
                                {
                                    yield return new Point3D((_xNotch + _xEdge) / 2, _outer, _zNotch);
                                }

                                // mid yNotch (1) @ Outer
                                if (Math.Abs(_zNotch - _zEdge) >= 2.5)
                                {
                                    yield return new Point3D(_xNotch, _outer, (_zNotch + _zEdge) / 2);
                                }

                                // mid anti-xNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_xNotch - _xEdge) <= 2.5)
                                {
                                    var _midPt = (_xNotch + _xAntiEdge) / 2;
                                    yield return new Point3D(_midPt, _inner, _zEdge);
                                    yield return new Point3D(_midPt, _outer, _zEdge);
                                    yield return new Point3D(_midPt, _midY, _zEdge);

                                    // irregular outer face
                                    yield return new Point3D(_midPt, _outer, 2.5);
                                }

                                // mid anti-zNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_zNotch - _zEdge) <= 2.5)
                                {
                                    var _midPt = (_zNotch + _zAntiEdge) / 2;
                                    yield return new Point3D(_xEdge, _inner, _midPt);
                                    yield return new Point3D(_xEdge, _outer, _midPt);
                                    yield return new Point3D(_xEdge, _midY, _midPt);

                                    // irregular outer face
                                    yield return new Point3D(2.5, _outer, _midPt);
                                }
                                #endregion
                            }
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            {
                                #region points for thick X
                                var _outer = (_thickFace == AnchorFace.ZLow) ? 0d : 5d;
                                var _inner = (_thickFace == AnchorFace.ZLow) ? _highX : _lowX;

                                // 3 pts @ inner at frameFace edges
                                if (!_outerCorner(Faces(AnchorFace.YLow, AnchorFace.ZLow, _thickFace)))
                                {
                                    yield return new Point3D(_inner, 0, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.YHigh, AnchorFace.ZLow, _thickFace)))
                                {
                                    yield return new Point3D(_inner, 5, 0);
                                }

                                if (!_outerCorner(Faces(AnchorFace.YLow, AnchorFace.ZHigh, _thickFace)))
                                {
                                    yield return new Point3D(_inner, 0, 5);
                                }

                                if (!_outerCorner(Faces(AnchorFace.YHigh, AnchorFace.ZHigh, _thickFace)))
                                {
                                    yield return new Point3D(_inner, 5, 5);
                                }

                                // 3 pts @ outer with FrameOffsets
                                yield return new Point3D(_outer, _yNotch, _zEdge);
                                yield return new Point3D(_outer, _yEdge, _zNotch);
                                yield return new Point3D(_outer, _yNotch, _zNotch);

                                // 2 pts @ inner with FrameOffsets
                                yield return new Point3D(_inner, _yNotch, _zEdge);
                                yield return new Point3D(_inner, _yEdge, _zNotch);

                                // 2 edges @ inner (on frameface)
                                if (_frameFaces.Contains(AnchorFace.YLow))
                                {
                                    yield return new Point3D(_inner, 0, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.YHigh))
                                {
                                    yield return new Point3D(_inner, 5, 2.5);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZLow))
                                {
                                    yield return new Point3D(_inner, 2.5, 0);
                                }

                                if (_frameFaces.Contains(AnchorFace.ZHigh))
                                {
                                    yield return new Point3D(_inner, 2.5, 5);
                                }

                                if (_xOff >= 2.5)
                                {
                                    // 2 faces on frameface
                                    if (_frameFaces.Contains(AnchorFace.YLow))
                                    {
                                        yield return new Point3D(_midX, 0, 2.5);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.YHigh))
                                    {
                                        yield return new Point3D(_midX, 5, 2.5);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.ZLow))
                                    {
                                        yield return new Point3D(_midX, 2.5, 0);
                                    }

                                    if (_frameFaces.Contains(AnchorFace.ZHigh))
                                    {
                                        yield return new Point3D(_midX, 2.5, 5);
                                    }

                                    // zNotch edge point
                                    yield return new Point3D(_midX, _yEdge, _zNotch);

                                    // yNotch edge point
                                    yield return new Point3D(_midX, _yNotch, _zEdge);
                                }

                                // mid xNotch (1) @ Outer
                                if (Math.Abs(_zNotch - _zEdge) >= 2.5)
                                {
                                    yield return new Point3D(_outer, _yNotch, (_zNotch + _zEdge) / 2);
                                }

                                // mid yNotch (1) @ Outer
                                if (Math.Abs(_yNotch - _yEdge) >= 2.5)
                                {
                                    yield return new Point3D(_outer, (_yNotch + _yEdge) / 2, _zNotch);
                                }

                                // mid anti-zNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_zNotch - _zEdge) <= 2.5)
                                {
                                    var _midPt = (_zNotch + _zAntiEdge) / 2;
                                    yield return new Point3D(_inner, _yEdge, _midPt);
                                    yield return new Point3D(_outer, _yEdge, _midPt);
                                    yield return new Point3D(_midX, _yEdge, _midPt);

                                    // irregular outer face
                                    yield return new Point3D(_outer, 2.5, _midPt);
                                }

                                // mid anti-yNotch edges and face (2 edges, 1 face)
                                if (Math.Abs(_yNotch - _yEdge) <= 2.5)
                                {
                                    var _midPt = (_yNotch + _yAntiEdge) / 2;
                                    yield return new Point3D(_inner, _midPt, _zEdge);
                                    yield return new Point3D(_outer, _midPt, _zEdge);
                                    yield return new Point3D(_midX, _midPt, _zEdge);

                                    // irregular outer face
                                    yield return new Point3D(_outer, _midPt, 2.5);
                                }
                                #endregion
                            }
                            break;
                    }
                }

                #region outer corners
                // regular outer corners
                if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow)))
                {
                    yield return new Point3D(0, 0, 0);
                }

                if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow)))
                {
                    yield return new Point3D(5, 0, 0);
                }

                if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow)))
                {
                    yield return new Point3D(0, 5, 0);
                }

                if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow)))
                {
                    yield return new Point3D(5, 5, 0);
                }

                if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(0, 0, 5);
                }

                if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(5, 0, 5);
                }

                if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(0, 5, 5);
                }

                if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(5, 5, 5);
                }
                #endregion

                #region regular edges
                // regular outer edges
                if (_regularEdge(Faces(AnchorFace.XLow, AnchorFace.ZLow)))
                {
                    yield return new Point3D(0, 2.5, 0);
                }

                if (_regularEdge(Faces(AnchorFace.XHigh, AnchorFace.ZLow)))
                {
                    yield return new Point3D(5, 2.5, 0);
                }

                if (_regularEdge(Faces(AnchorFace.YLow, AnchorFace.ZLow)))
                {
                    yield return new Point3D(2.5, 0, 0);
                }

                if (_regularEdge(Faces(AnchorFace.YHigh, AnchorFace.ZLow)))
                {
                    yield return new Point3D(2.5, 5, 0);
                }

                if (_regularEdge(Faces(AnchorFace.XLow, AnchorFace.YLow)))
                {
                    yield return new Point3D(0, 0, 2.5);
                }

                if (_regularEdge(Faces(AnchorFace.XHigh, AnchorFace.YLow)))
                {
                    yield return new Point3D(5, 0, 2.5);
                }

                if (_regularEdge(Faces(AnchorFace.XLow, AnchorFace.YHigh)))
                {
                    yield return new Point3D(0, 5, 2.5);
                }

                if (_regularEdge(Faces(AnchorFace.XHigh, AnchorFace.YHigh)))
                {
                    yield return new Point3D(5, 5, 2.5);
                }

                if (_regularEdge(Faces(AnchorFace.XLow, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(0, 2.5, 5);
                }

                if (_regularEdge(Faces(AnchorFace.XHigh, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(5, 2.5, 5);
                }

                if (_regularEdge(Faces(AnchorFace.YLow, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(2.5, 0, 5);
                }

                if (_regularEdge(Faces(AnchorFace.YHigh, AnchorFace.ZHigh)))
                {
                    yield return new Point3D(2.5, 5, 5);
                }
                #endregion

                bool _irrOuterEdge(IEnumerable<AnchorFace> faces, double offset)
                {
                    // irregular outer edges are at least 2.5 units long have neither thickFace nor openFace, and at least one frameface
                    return (offset >= 2.5) && !faces.Contains(_thickFace) && !faces.Contains(_openFace) && faces.Intersect(_frameFaces).Any();
                }

                #region irregular outer edges
                if (_irrOuterEdge(Faces(AnchorFace.XLow, AnchorFace.ZLow), _yOff))
                {
                    yield return new Point3D(0, _midY, 0);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XHigh, AnchorFace.ZLow), _yOff))
                {
                    yield return new Point3D(5, _midY, 0);
                }

                if (_irrOuterEdge(Faces(AnchorFace.YLow, AnchorFace.ZLow), _xOff))
                {
                    yield return new Point3D(_midX, 0, 0);
                }

                if (_irrOuterEdge(Faces(AnchorFace.YHigh, AnchorFace.ZLow), _xOff))
                {
                    yield return new Point3D(_midX, 5, 0);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XLow, AnchorFace.YLow), _zOff))
                {
                    yield return new Point3D(0, 0, _midZ);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XHigh, AnchorFace.YLow), _zOff))
                {
                    yield return new Point3D(5, 0, _midZ);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XLow, AnchorFace.YHigh), _zOff))
                {
                    yield return new Point3D(0, 5, _midZ);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XHigh, AnchorFace.YHigh), _zOff))
                {
                    yield return new Point3D(5, 5, _midZ);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XLow, AnchorFace.ZHigh), _yOff))
                {
                    yield return new Point3D(0, _midY, 5);
                }

                if (_irrOuterEdge(Faces(AnchorFace.XHigh, AnchorFace.ZHigh), _yOff))
                {
                    yield return new Point3D(5, _midY, 5);
                }

                if (_irrOuterEdge(Faces(AnchorFace.YLow, AnchorFace.ZHigh), _xOff))
                {
                    yield return new Point3D(_midX, 0, 5);
                }

                if (_irrOuterEdge(Faces(AnchorFace.YHigh, AnchorFace.ZHigh), _xOff))
                {
                    yield return new Point3D(_midX, 5, 5);
                }
                #endregion

            }
            yield break;
        }
        #endregion

        #region public override uint FlipParameters(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            var _thickFace = LFrameSpaceFaces.GetThickFace(paramsIn);
            var _frame1Face = LFrameSpaceFaces.GetFrame1Face(paramsIn);
            var _frame2Face = LFrameSpaceFaces.GetFrame2Face(paramsIn);
            if (_thickFace.GetAxis() == flipAxis)
            {
                return LFrameSpaceFaces.GetParam(_thickFace.ReverseFace(), _frame1Face, _frame2Face);
            }
            else if (_frame1Face.GetAxis() == flipAxis)
            {
                return LFrameSpaceFaces.GetParam(_thickFace, _frame1Face.ReverseFace(), _frame2Face);
            }
            else if (_frame2Face.GetAxis() == flipAxis)
            {
                return LFrameSpaceFaces.GetParam(_thickFace, _frame1Face, _frame2Face.ReverseFace());
            }
            else
            {
                // ?!?
                return paramsIn;
            }
        }
        #endregion

        #region public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            if (axis1 != axis2)
            {
                var _thickFace = LFrameSpaceFaces.GetThickFace(paramsIn);
                var _thickLow = _thickFace.IsLowFace();
                var _thickAxis = _thickFace.GetAxis();

                var _frame1Face = LFrameSpaceFaces.GetFrame1Face(paramsIn);
                var _f1Low = _frame1Face.IsLowFace();
                var _f1Axis = _frame1Face.GetAxis();

                var _frame2Face = LFrameSpaceFaces.GetFrame2Face(paramsIn);
                var _f2Low = _frame2Face.IsLowFace();
                var _f2Axis = _frame2Face.GetAxis();

                if (axis1 == _thickAxis)
                {
                    if (axis2 == _f1Axis)
                    {
                        // thick and f1
                        return LFrameSpaceFaces.GetParam(
                            _thickLow ? _f1Axis.GetLowFace() : _f1Axis.GetHighFace(),
                            _f1Low ? _thickAxis.GetLowFace() : _thickAxis.GetHighFace(),
                            _frame2Face);
                    }

                    // thick and f2
                    return LFrameSpaceFaces.GetParam(
                        _thickLow ? _f2Axis.GetLowFace() : _f2Axis.GetHighFace(),
                        _frame1Face,
                        _f2Low ? _thickAxis.GetLowFace() : _thickAxis.GetHighFace());
                }
                else if (axis1 == _f1Axis)
                {
                    if (axis2 == _thickAxis)
                    {
                        // thick and f1
                        return LFrameSpaceFaces.GetParam(
                            _thickLow ? _f1Axis.GetLowFace() : _f1Axis.GetHighFace(),
                            _f1Low ? _thickAxis.GetLowFace() : _thickAxis.GetHighFace(),
                            _frame2Face);
                    }

                    // f1 and f2
                    return LFrameSpaceFaces.GetParam(
                        _thickFace,
                        _f1Low ? _f2Axis.GetLowFace() : _f2Axis.GetHighFace(),
                        _f2Low ? _f1Axis.GetLowFace() : _f1Axis.GetHighFace());
                }
                else
                {
                    if (axis2 == _thickAxis)
                    {
                        // thick and f2
                        return LFrameSpaceFaces.GetParam(
                            _thickLow ? _f2Axis.GetLowFace() : _f2Axis.GetHighFace(),
                            _frame1Face,
                            _f2Low ? _thickAxis.GetLowFace() : _thickAxis.GetHighFace());
                    }

                    // f1 and f2
                    return LFrameSpaceFaces.GetParam(
                        _thickFace,
                        _f1Low ? _f2Axis.GetLowFace() : _f2Axis.GetHighFace(),
                        _f2Low ? _f1Axis.GetLowFace() : _f1Axis.GetHighFace());
                }
            }
            return paramsIn;
        }
        #endregion

        #region public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                // nothing
            }
            else if (!_cellBlock && !_plusBlock)
            {
                foreach (var _pt in base.TargetCorners(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                var _thickFace = LFrameSpaceFaces.GetThickFace(param);
                var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
                var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
                var _frameFaces = Faces(_frame1Face, _frame2Face).ToList();

                // critical values
                var _lowZ = 0d;
                var _lowY = 0d;
                var _lowX = 0d;
                var _highZ = 5d;
                var _highY = 5d;
                var _highX = 5d;

                double _zOff;
                double _yOff;
                double _xOff;

                if (_cellBlock)
                {
                    #region solid "L"

                    #region adjust for thickface
                    // adjust irregular edges
                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                            _lowZ = Thickness;
                            break;
                        case AnchorFace.ZHigh:
                            _highZ -= Thickness;
                            break;
                        case AnchorFace.YLow:
                            _lowY = Thickness;
                            break;
                        case AnchorFace.YHigh:
                            _highY -= Thickness;
                            break;
                        case AnchorFace.XLow:
                            _lowX = Thickness;
                            break;
                        case AnchorFace.XHigh:
                            _highX -= Thickness;
                            break;
                    }
                    #endregion

                    #region adjust for notch
                    if (_frameFaces.Contains(AnchorFace.ZLow))
                    {
                        if (_frame1Face == AnchorFace.ZLow)
                        {
                            _lowZ = Width1;
                        }
                        else
                        {
                            _lowZ = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.ZHigh))
                    {
                        if (_frame1Face == AnchorFace.ZHigh)
                        {
                            _highZ -= Width1;
                        }
                        else
                        {
                            _highZ -= Width2;
                        }
                    }
                    if (_frameFaces.Contains(AnchorFace.YLow))
                    {
                        if (_frame1Face == AnchorFace.YLow)
                        {
                            _lowY = Width1;
                        }
                        else
                        {
                            _lowY = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.YHigh))
                    {
                        if (_frame1Face == AnchorFace.YHigh)
                        {
                            _highY -= Width1;
                        }
                        else
                        {
                            _highY -= Width2;
                        }
                    }
                    if (_frameFaces.Contains(AnchorFace.XLow))
                    {
                        if (_frame1Face == AnchorFace.XLow)
                        {
                            _lowX = Width1;
                        }
                        else
                        {
                            _lowX = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.XHigh))
                    {
                        if (_frame1Face == AnchorFace.XHigh)
                        {
                            _highX -= Width1;
                        }
                        else
                        {
                            _highX -= Width2;
                        }
                    }
                    #endregion

                    #region calc dimensional extents
                    _zOff = _highZ - _lowZ;
                    _yOff = _highY - _lowY;
                    _xOff = _highX - _lowX;
                    #endregion

                    #region all outer corners
                    // declare functions
                    bool _outerCorner(IEnumerable<AnchorFace> faces)
                    {
                        // non thickFace corners have openCorners (which are good) ...
                        // ... the only thickCorner that is good has no frameFaces
                        return !faces.Contains(_thickFace) || !faces.Intersect(_frameFaces).Any();
                    }
                    List<TargetCorner> _allOuterCorners()
                    {
                        var _all = new List<TargetCorner>();
                        // regular outer corners
                        if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow)))
                        {
                            _all.Add(new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow));
                        }

                        if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow)))
                        {
                            _all.Add(new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow));
                        }

                        if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow)))
                        {
                            _all.Add(new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow));
                        }

                        if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow)))
                        {
                            _all.Add(new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow));
                        }

                        if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh)))
                        {
                            _all.Add(new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh));
                        }

                        if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh)))
                        {
                            _all.Add(new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh));
                        }

                        if (_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh)))
                        {
                            _all.Add(new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh));
                        }

                        if (_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh)))
                        {
                            _all.Add(new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh));
                        }

                        return _all;
                    }
                    #endregion

                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            {
                                // notch or sliver has more volume?
                                if ((_xOff * _yOff * 5) > (25 * _zOff))
                                {
                                    #region notch
                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region sliver
                                    var _inner = (_thickFace == AnchorFace.ZLow) ? _lowZ : _highZ;

                                    // 3 pts @ inner at frameFace edges
                                    if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YLow, _thickFace)))
                                    {
                                        yield return new TargetCorner(new Point3D(0, 0, _inner),
                                            AnchorFace.XLow, AnchorFace.YLow, _thickFace);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YLow, _thickFace)))
                                    {
                                        yield return new TargetCorner(new Point3D(5, 0, _inner),
                                            AnchorFace.XHigh, AnchorFace.YLow, _thickFace);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XLow, AnchorFace.YHigh, _thickFace)))
                                    {
                                        yield return new TargetCorner(new Point3D(0, 5, _inner),
                                            AnchorFace.XLow, AnchorFace.YHigh, _thickFace);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XHigh, AnchorFace.YHigh, _thickFace)))
                                    {
                                        yield return new TargetCorner(new Point3D(5, 5, _inner),
                                            AnchorFace.XHigh, AnchorFace.YHigh, _thickFace);
                                    }

                                    foreach (var _c in _allOuterCorners())
                                    {
                                        yield return _c;
                                    }
                                    #endregion
                                }
                            }
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            {
                                // notch or sliver has more volume?
                                if ((_xOff * _zOff * 5) > (25 * _yOff))
                                {
                                    #region notch
                                    yield return new TargetCorner(new Point3D(_lowX, 0, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, 0, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region sliver
                                    var _inner = (_thickFace == AnchorFace.YLow) ? _lowY : _highY;

                                    // 3 pts @ lowZ at frameFace edges
                                    if (!_outerCorner(Faces(AnchorFace.XLow, _thickFace, AnchorFace.ZLow)))
                                    {
                                        yield return new TargetCorner(new Point3D(0, _inner, 0),
                                            AnchorFace.XLow, _thickFace, AnchorFace.ZLow);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XHigh, _thickFace, AnchorFace.ZLow)))
                                    {
                                        yield return new TargetCorner(new Point3D(5, _inner, 0),
                                            AnchorFace.XHigh, _thickFace, AnchorFace.ZLow);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XLow, _thickFace, AnchorFace.ZHigh)))
                                    {
                                        yield return new TargetCorner(new Point3D(0, _inner, 5),
                                            AnchorFace.XLow, _thickFace, AnchorFace.ZHigh);
                                    }

                                    if (!_outerCorner(Faces(AnchorFace.XHigh, _thickFace, AnchorFace.ZHigh)))
                                    {
                                        yield return new TargetCorner(new Point3D(5, _inner, 5),
                                            AnchorFace.XHigh, _thickFace, AnchorFace.ZHigh);
                                    }

                                    foreach (var _c in _allOuterCorners())
                                    {
                                        yield return _c;
                                    }
                                    #endregion
                                }
                            }
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            {
                                // notch or sliver has more volume?
                                if ((_yOff * _zOff * 5) > (25 * _xOff))
                                {
                                    #region notch
                                    yield return new TargetCorner(new Point3D(0, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(0, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(0, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(0, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region sliver
                                    var _inner = (_thickFace == AnchorFace.XLow) ? _lowX : _highX;

                                    // 3 pts @ inner at frameFace edges
                                    if (!_outerCorner(Faces(_thickFace, AnchorFace.YLow, AnchorFace.ZLow)))
                                    {
                                        yield return new TargetCorner(new Point3D(_inner, 0, 0),
                                            _thickFace, AnchorFace.YLow, AnchorFace.ZLow);
                                    }

                                    if (!_outerCorner(Faces(_thickFace, AnchorFace.YHigh, AnchorFace.ZLow)))
                                    {
                                        yield return new TargetCorner(new Point3D(_inner, 5, 0),
                                            _thickFace, AnchorFace.YHigh, AnchorFace.ZLow);
                                    }

                                    if (!_outerCorner(Faces(_thickFace, AnchorFace.YLow, AnchorFace.ZHigh)))
                                    {
                                        yield return new TargetCorner(new Point3D(_inner, 0, 5),
                                            _thickFace, AnchorFace.YLow, AnchorFace.ZHigh);
                                    }

                                    if (!_outerCorner(Faces(_thickFace, AnchorFace.YHigh, AnchorFace.ZHigh)))
                                    {
                                        yield return new TargetCorner(new Point3D(_inner, 5, 5),
                                            _thickFace, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }

                                    foreach (var _c in _allOuterCorners())
                                    {
                                        yield return _c;
                                    }
                                    #endregion
                                }
                            }
                            break;
                    }
                    #endregion
                }
                else
                {
                    // hollow "L"

                    #region adjust for thickface
                    // adjust irregular edges
                    switch (_thickFace)
                    {
                        case AnchorFace.ZHigh:
                            _lowZ = 5 - Thickness;
                            break;
                        case AnchorFace.ZLow:
                            _highZ = Thickness;
                            break;
                        case AnchorFace.YHigh:
                            _lowY = 5 - Thickness;
                            break;
                        case AnchorFace.YLow:
                            _highY = Thickness;
                            break;
                        case AnchorFace.XHigh:
                            _lowX = 5 - Thickness;
                            break;
                        case AnchorFace.XLow:
                            _highX = Thickness;
                            break;
                    }
                    #endregion

                    #region adjust for notch
                    if (_frameFaces.Contains(AnchorFace.ZLow))
                    {
                        if (_frame1Face == AnchorFace.ZLow)
                        {
                            _highZ = Width1;
                        }
                        else
                        {
                            _highZ = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.ZHigh))
                    {
                        if (_frame1Face == AnchorFace.ZHigh)
                        {
                            _lowZ = 5 - Width1;
                        }
                        else
                        {
                            _lowZ = 5 - Width2;
                        }
                    }
                    if (_frameFaces.Contains(AnchorFace.YLow))
                    {
                        if (_frame1Face == AnchorFace.YLow)
                        {
                            _highY = Width1;
                        }
                        else
                        {
                            _highY = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.YHigh))
                    {
                        if (_frame1Face == AnchorFace.YHigh)
                        {
                            _lowY = 5 - Width1;
                        }
                        else
                        {
                            _lowY = 5 - Width2;
                        }
                    }
                    if (_frameFaces.Contains(AnchorFace.XLow))
                    {
                        if (_frame1Face == AnchorFace.XLow)
                        {
                            _highX = Width1;
                        }
                        else
                        {
                            _highX = Width2;
                        }
                    }
                    else if (_frameFaces.Contains(AnchorFace.XHigh))
                    {
                        if (_frame1Face == AnchorFace.XHigh)
                        {
                            _lowX = 5 - Width1;
                        }
                        else
                        {
                            _lowX = 5 - Width2;
                        }
                    }
                    #endregion

                    #region calc dimensional extents
                    _zOff = _highZ - _lowZ;
                    _yOff = _highY - _lowY;
                    _xOff = _highX - _lowX;
                    #endregion

                    switch (_thickFace)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            {
                                if ((Width1 > 2.5) && (Width2 > 2.5))
                                {
                                    #region warped corner
                                    // blocking notch is "smallish"
                                    if ((_lowX == 0) || (_lowY == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(0, 0, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(0, 0, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    }

                                    if ((_highX == 5) || (_lowY == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(5, 0, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(5, 0, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    }

                                    if ((_lowX == 0) || (_highY == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(0, 5, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(0, 5, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }

                                    if ((_highX == 5) || (_highY == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(5, 5, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(5, 5, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    #endregion
                                }
                                else if (_xOff >= _yOff)
                                {
                                    #region x by 5
                                    yield return new TargetCorner(new Point3D(_lowX, 0, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(_lowX, 0, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region y by 5
                                    yield return new TargetCorner(new Point3D(0, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(0, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(0, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(0, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                            }
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            {
                                if ((Width1 > 2.5) && (Width2 > 2.5))
                                {
                                    #region warped corner
                                    // blocking notch is "smallish"
                                    if ((_lowX == 0) || (_lowZ == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(0, _lowY, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(0, _highY, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    }

                                    if ((_highX == 5) || (_lowZ == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(5, _lowY, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(5, _highY, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                                    }

                                    if ((_lowX == 0) || (_highZ == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(0, _lowY, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                        yield return new TargetCorner(new Point3D(0, _highY, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }

                                    if ((_highX == 5) || (_highZ == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(5, _lowY, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                        yield return new TargetCorner(new Point3D(5, _highY, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    }
                                    #endregion
                                }
                                else if (_xOff >= _zOff)
                                {
                                    #region x by 5
                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region z by 5
                                    yield return new TargetCorner(new Point3D(0, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(0, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(5, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(0, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(0, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(5, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                            }
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            {
                                if ((Width1 > 2.5) && (Width2 > 2.5))
                                {
                                    #region warped corner
                                    // blocking notch is "smallish"
                                    if ((_lowY == 0) || (_lowZ == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(_lowX, 0, 0), AnchorFace.YLow, AnchorFace.ZLow, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, 0, 0), AnchorFace.YLow, AnchorFace.ZLow, AnchorFace.XHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _lowZ), AnchorFace.YLow, AnchorFace.ZLow, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _lowZ), AnchorFace.YLow, AnchorFace.ZLow, AnchorFace.XHigh);
                                    }

                                    if ((_highY == 5) || (_lowZ == 0))
                                    {
                                        yield return new TargetCorner(new Point3D(_lowX, 5, 0), AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, 5, 0), AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.XHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _lowZ), AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _lowZ), AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.XHigh);
                                    }

                                    if ((_lowY == 0) || (_highZ == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(_lowX, 0, 5), AnchorFace.YLow, AnchorFace.ZHigh, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, 0, 5), AnchorFace.YLow, AnchorFace.ZHigh, AnchorFace.XHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _highZ), AnchorFace.YLow, AnchorFace.ZHigh, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, _lowY, _highZ), AnchorFace.YLow, AnchorFace.ZHigh, AnchorFace.XHigh);
                                    }

                                    if ((_highY == 5) || (_highZ == 5))
                                    {
                                        yield return new TargetCorner(new Point3D(_lowX, 5, 5), AnchorFace.YHigh, AnchorFace.ZHigh, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, 5, 5), AnchorFace.YHigh, AnchorFace.ZHigh, AnchorFace.XHigh);
                                    }
                                    else
                                    {
                                        // odd point
                                        yield return new TargetCorner(new Point3D(_lowX, _highY, _highZ), AnchorFace.YHigh, AnchorFace.ZHigh, AnchorFace.XLow);
                                        yield return new TargetCorner(new Point3D(_highX, _highY, _highZ), AnchorFace.YHigh, AnchorFace.ZHigh, AnchorFace.XHigh);
                                    }
                                    #endregion
                                }
                                else if (_yOff >= _zOff)
                                {
                                    #region y by 5
                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(_lowX, _lowY, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _lowY, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, _highY, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, _highY, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                                else
                                {
                                    #region z by 5
                                    yield return new TargetCorner(new Point3D(_lowX, 0, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                                    yield return new TargetCorner(new Point3D(_lowX, 0, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 0, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_lowX, 5, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    yield return new TargetCorner(new Point3D(_highX, 5, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                                    #endregion
                                }
                            }
                            break;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (surfaceFace == LFrameSpaceFaces.GetThickFace(param))
            {
                var _cellBlock = !movement.CanMoveThrough(CellMaterial);
                if (_cellBlock && _plusBlock)
                {
                    // both block
                    return new HedralGrip(true);
                }
                else if (!_cellBlock && !_plusBlock)
                {
                    // neither block
                    return new HedralGrip(false);
                }
                else
                {
                    var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
                    var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
                    var _moveAxis = surfaceFace.GetAxis();

                    /// coverage of the plus area
                    var _plusCover = ((5 - Width1) * (5 - Width2)) / 25d;
                    if (_plusBlock)
                    {
                        // since plus blocks use if as coverage
                        return (new HedralGrip(_moveAxis, _frame1Face.ReverseFace(), Width1))
                            .Intersect(new HedralGrip(_moveAxis, _frame2Face.ReverseFace(), Width2));
                    }
                    else
                    {
                        // since plus doesn't block, difference is the coverage
                        return (new HedralGrip(_moveAxis, _frame1Face, Width1))
                            .Union(new HedralGrip(_moveAxis, _frame2Face, Width2));
                    }
                }
            }
            else
            {
                // base blocking (plus fills out volume)
                return new HedralGrip(_plusBlock);
            }
        }
        #endregion

        // TODO:

        #region public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock ^ _plusBlock)
            {
                // only one material blocks
                var _thick = LFrameSpaceFaces.GetThickFace(param);
                var _x = 0d;
                var _y = 0d;
                var _z = 0d;
                if (_plusBlock)
                {
                    switch (_thick)
                    {
                        case AnchorFace.ZLow: _z += Thickness; break;
                        case AnchorFace.YLow: _y += Thickness; break;
                        case AnchorFace.XLow: _x += Thickness; break;
                        case AnchorFace.ZHigh: _z -= Thickness; break;
                        case AnchorFace.YHigh: _y -= Thickness; break;
                        case AnchorFace.XHigh: _x -= Thickness; break;
                        default: break;
                    }
                }
                else
                {
                    switch (_thick)
                    {
                        case AnchorFace.ZLow: _z -= Thickness; break;
                        case AnchorFace.YLow: _y -= Thickness; break;
                        case AnchorFace.XLow: _x -= Thickness; break;
                        case AnchorFace.ZHigh: _z += Thickness; break;
                        case AnchorFace.YHigh: _y += Thickness; break;
                        case AnchorFace.XHigh: _x += Thickness; break;
                        default: break;
                    }
                }
                return new Vector3D(_x, _y, _z);
            }
            return new Vector3D();
        }
        #endregion

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => LFrameSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => LFrameSpaceFaces.ShowFace(param, this, outwardFace);

        public override CellSpaceInfo ToCellSpaceInfo()
            => new LFrameSpaceInfo(this);

        #region public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);

            if (_cellBlock ^ _plusBlock)
            {
                var _thickFace = LFrameSpaceFaces.GetThickFace(param);
                var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
                var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
                var _faces = (_cellBlock
                       ? _thickFace
                       : _thickFace.ReverseFace()).ToAnchorFaceList();

                // if gravity matches one of the faces with _cellBlock, then ledge
                // ...same if gravity is opposite one of the faces with _plusBlock
                if ((_cellBlock && ((gravity == _thickFace) || (gravity == _frame1Face) || (gravity == _frame2Face)))
                    || (_plusBlock && ((gravity == _thickFace.ReverseFace()) || (gravity == _frame1Face.ReverseFace()) || (gravity == _frame2Face.ReverseFace()))))
                {
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerLedge(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }

                // if gravity matches one of the faces with _plusBlock, then dangle
                // ...same if gravity is opposite one of the faces with _cellBlock
                else if ((_plusBlock && ((gravity == _thickFace) || (gravity == _frame1Face) || (gravity == _frame2Face)))
                    || (_cellBlock && ((gravity == _thickFace.ReverseFace()) || (gravity == _frame1Face.ReverseFace()) || (gravity == _frame2Face.ReverseFace()))))
                {
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerDangling(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }

                // all others use base
                return new CellGripResult
                {
                    Difficulty = GripRules.GetInnerBase(_plusBlock),
                    Faces = _faces,
                    InnerFaces = _faces
                };
            }

            // no grip
            return new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
        }
        #endregion

        #region public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            // disposition
            var _thickFace = LFrameSpaceFaces.GetThickFace(param);
            var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
            var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
            var _revThick = _thickFace.ReverseFace();
            var _revGravity = gravity.ReverseFace();
            var _disposition = (gripFace == _revThick) ? GripDisposition.Full
                : ((gripFace == _frame1Face) || (gripFace == _frame2Face)) ? GripDisposition.Rectangular
                : GripDisposition.Irregular;

            if (gravity == gripFace)
            {
                // dangling...
                if (_disposition == GripDisposition.Full)
                {
                    return GripRules.GetOuterDangling(true);
                }

                return GripRules.GetOuterDangling(_disposition);
            }

            // full base grip?
            if (_disposition == GripDisposition.Full)
            {
                // ... always plus material
                return GripRules.GetOuterBase(true);
            }

            // can only be ledgy if a single material blocks
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock ^ _plusBlock)
            {
                if (gripFace == _thickFace)
                {
                    // gripping the lframe face
                    if ((_cellBlock && ((gravity == _frame1Face) || (gravity == _frame2Face)))
                        || (_plusBlock && ((_revGravity == _frame1Face) || (_revGravity == _frame2Face)))
                        )
                    {
                        // ledges...
                        return GripRules.GetOuterLedge(_plusBlock, _disposition);
                    }
                }
                else if ((_cellBlock && (gravity == _thickFace))
                    || (_plusBlock && (gravity == _revThick)))
                {
                    // ledges...
                    return GripRules.GetOuterLedge(_plusBlock, _disposition);
                }
            }

            // mixed face base grip
            return GripRules.GetOuterBase(_disposition);
        }
        #endregion

        public override int? InnerSwimDifficulty(uint param)
            => (new[] { base.InnerSwimDifficulty(param), MaterialSwimDifficulty(PlusMaterial) }).Max();
    }
}
