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
    public class Stairs : ComponentSpace, IStairs
    {
        public Stairs(CellMaterial material, TileSet tileSet, CellMaterial plusMaterial, TileSet plusTiles, int steps)
            : base(material, tileSet, plusMaterial, plusTiles)
        {
            _Steps = steps;
        }

        #region private data
        private int _Steps;
        #endregion

        #region public int Steps { get; set; }
        public int Steps
        {
            get { return _Steps; }
            set
            {
                if (_Steps != value)
                {
                    _Steps = value;
                    DoPropertyChanged(@"Steps");
                }
            }
        }
        #endregion

        public override string GetDescription(uint param)
            => $@"Stairs:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName}) [#={Steps}]";

        public override string GetParamText(uint param)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
            return $@"Climb={_climbOpen}, Travel={_travelOpen}";
        }

        #region protected override IEnumerable<CellStructure> Components(uint param)
        protected override IEnumerable<CellStructure> Components(uint param)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);

            #region climb base
            if (!_climbOpen.IsLowFace())
            {
                var _sliver = new SliverCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling) { IsShell = true };
                yield return new CellStructure(
                    _sliver,
                    (new SliverSlopeParams { Axis = _climbOpen.GetAxis(), Flip = false, OffsetUnits = 0 }).Value);
            }
            else
            {
                var _sliver = new SliverCellSpace(PlusMaterial, PlusTiling, CellMaterial, Tiling) { IsShell = true };
                yield return new CellStructure(
                    _sliver,
                    (new SliverSlopeParams { Axis = _climbOpen.GetAxis(), Flip = false, OffsetUnits = 60 }).Value);
            }
            #endregion

            #region travel through base
            if (!_travelOpen.IsLowFace())
            {
                var _sliver = new SliverCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling) { IsShell = true };
                yield return new CellStructure(
                    _sliver,
                    (new SliverSlopeParams { Axis = _travelOpen.GetAxis(), Flip = false, OffsetUnits = 0 }).Value);
            }
            else
            {
                var _sliver = new SliverCellSpace(PlusMaterial, PlusTiling, CellMaterial, Tiling) { IsShell = true };
                yield return new CellStructure(
                    _sliver,
                    (new SliverSlopeParams { Axis = _travelOpen.GetAxis(), Flip = false, OffsetUnits = 60 }).Value);
            }
            #endregion

            var _wedgeParam = StairSpaceFaces.WedgeParallelParam(_climbOpen, _travelOpen);
            var _stepSize = 5d / Steps;
            var _secondary = 5d - _stepSize;
            var _primary = _stepSize;
            for (var _sx = 0; _sx < Steps - 1; _sx++)
            {
                var _wedge = new CornerCellSpace(CellMaterial, Tiling, PlusMaterial, PlusTiling, _primary, _secondary);
                yield return new CellStructure(_wedge, _wedgeParam);
                _primary += _stepSize;
                _secondary -= _stepSize;
            }
            yield break;
        }
        #endregion

        #region public override Hedralgrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
            if ((surfaceFace == _climbOpen) || (surfaceFace == _travelOpen))
            {
                return new HedralGrip(!movement.CanMoveThrough(PlusMaterial));
            }

            if ((surfaceFace == _climbOpen.ReverseFace()) || (surfaceFace == _travelOpen.ReverseFace()))
            {
                return new HedralGrip(!movement.CanMoveThrough(CellMaterial));
            }

            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);

            if (_cellBlock && _plusBlock)
            {
                return new HedralGrip(true);
            }
            else if (!_cellBlock && !_plusBlock)
            {
                return new HedralGrip(false);
            }
            else
            {
                if (_cellBlock)
                {
                    bool _unblocked(AnchorFace face)
                        => _travelOpen == face || _climbOpen == face;

                    HedralGrip _grip(AnchorFace leftFace, AnchorFace lowerFace)
                    {
                        if (_unblocked(leftFace))
                        {
                            if (_unblocked(lowerFace))
                            {
                                return new HedralGrip(TriangleCorner.LowerLeft);
                            }
                            else
                            {
                                return new HedralGrip(TriangleCorner.UpperLeft);
                            }
                        }

                        if (_unblocked(lowerFace))
                        {
                            return new HedralGrip(TriangleCorner.LowerRight);
                        }
                        else
                        {
                            return new HedralGrip(TriangleCorner.UpperRight);
                        }
                    }

                    // only cell blocks
                    switch (surfaceFace.GetAxis())
                    {
                        case Axis.X:
                            return _grip(AnchorFace.YHigh, AnchorFace.ZHigh);

                        case Axis.Y:
                            return _grip(AnchorFace.ZHigh, AnchorFace.XHigh);

                        case Axis.Z:
                        default:
                            return _grip(AnchorFace.XHigh, AnchorFace.YHigh);
                    }
                }
                else
                {
                    bool _unblocked2(AnchorFace face)
                       => _travelOpen != face && _climbOpen != face;

                    HedralGrip _grip2(AnchorFace leftFace, AnchorFace lowerFace)
                    {
                        if (_unblocked2(leftFace))
                        {
                            if (_unblocked2(lowerFace))
                            {
                                return new HedralGrip(TriangleCorner.LowerLeft);
                            }
                            else
                            {
                                return new HedralGrip(TriangleCorner.UpperLeft);
                            }
                        }

                        if (_unblocked2(lowerFace))
                        {
                            return new HedralGrip(TriangleCorner.LowerRight);
                        }
                        else
                        {
                            return new HedralGrip(TriangleCorner.UpperRight);
                        }
                    }

                    // only plus blocks
                    switch (surfaceFace.GetAxis())
                    {
                        case Axis.X:
                            return _grip2(AnchorFace.YHigh, AnchorFace.ZHigh);

                        case Axis.Y:
                            return _grip2(AnchorFace.ZHigh, AnchorFace.XHigh);

                        case Axis.Z:
                        default:
                            return _grip2(AnchorFace.XHigh, AnchorFace.YHigh);
                    }
                }
            }
        }
        #endregion

        #region public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // TODO: may want to exclude the opening that is NOT aligned with gravity
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);

            if (movement.CanMoveThrough(CellMaterial))
            {
                yield return new MovementOpening(_climbOpen.ReverseFace(), 2.5, 1);
                yield return new MovementOpening(_travelOpen.ReverseFace(), 2.5, 1);
            }
            else if (movement.CanMoveThrough(PlusMaterial))
            {
                yield return new MovementOpening(_climbOpen, 2.5, 1);
                yield return new MovementOpening(_travelOpen, 2.5, 1);
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);

            var _elev = 0d;
            var _stepSize = 5d / Steps;
            if (((upDirection == _climbOpen) && movement.CanMoveThrough(PlusMaterial))
                || ((upDirection == _climbOpen.ReverseFace()) && movement.CanMoveThrough(CellMaterial))
                || ((upDirection == _travelOpen) && movement.CanMoveThrough(PlusMaterial))
                || ((upDirection == _travelOpen.ReverseFace()) && movement.CanMoveThrough(CellMaterial)))
            {
                for (var _sx = 0; _sx < Steps; _sx++)
                {
                    yield return new SlopeSegment
                    {
                        Low = _elev + baseElev,
                        High = _elev + baseElev,
                        Run = _stepSize
                    };
                    _elev += _stepSize;
                }
            }
            yield break;
        }
        #endregion

        #region public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        {
            var _cellMove = movement.CanMoveThrough(CellMaterial);
            var _plusMove = movement.CanMoveThrough(PlusMaterial);
            if (_plusMove ^ _cellMove)
            {
                var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
                var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
                var _cAxis = _climbOpen.GetAxis();
                var _tAxis = _travelOpen.GetAxis();
                var _gAxis = gravity.GetAxis();
                if ((_cAxis == _gAxis) || (_tAxis == _gAxis))
                {
                    // somewhat aligned with gravity
                    double _ord1(Axis axis) =>
                        (axis != _gAxis)
                        ? 0d
                        : (axis == _cAxis)
                        ? ((_climbOpen.IsLowFace() ^ _cellMove) ? -2.5d : 2.5d)
                        : (axis == _tAxis)
                        ? ((_travelOpen.IsLowFace() ^ _cellMove) ? -2.5d : 2.5d)
                        : 0d;
                    return new Vector3D(_ord1(Axis.X), _ord1(Axis.Y), _ord1(Axis.Z));
                }
                else
                {
                    // not aligned with gravity
                    double _ord2(Axis axis) =>
                        (axis == _cAxis)
                        ? ((_climbOpen.IsLowFace() ^ _cellMove) ? -2.5d : 2.5d)
                        : (axis == _tAxis)
                        ? ((_travelOpen.IsLowFace() ^ _cellMove) ? -2.5d : 2.5d)
                        : 0d;
                    return new Vector3D(_ord2(Axis.X), _ord2(Axis.Y), _ord2(Axis.Z));
                }
            }
            return new Vector3D();
        }
        #endregion

        #region public override Vector3D InteractionOffset3D(uint param)
        public override Vector3D InteractionOffset3D(uint param)
        {
            var _open = StairSpaceFaces.GetClimbOpening(param).ToAnchorFaceList();
            _open = _open.Add(StairSpaceFaces.GetTravelOpening(param));
            var _cellBlock = CellMaterial.BlocksEffect;
            var _plusBlock = PlusMaterial.BlocksEffect;
            var _vec = new Vector3D();
            if (_cellBlock ^ _plusBlock)
            {
                if (_cellBlock)
                {
                    if (_open.Intersects(AnchorFaceList.XHigh))
                    {
                        _vec += new Vector3D(1.25, 0, 0);
                    }
                    else if (_open.Intersects(AnchorFaceList.XLow))
                    {
                        _vec -= new Vector3D(1.25, 0, 0);
                    }

                    if (_open.Intersects(AnchorFaceList.YHigh))
                    {
                        _vec += new Vector3D(0, 1.25, 0);
                    }
                    else if (_open.Intersects(AnchorFaceList.YLow))
                    {
                        _vec -= new Vector3D(0, 1.25, 0);
                    }

                    if (_open.Intersects(AnchorFaceList.ZHigh))
                    {
                        _vec += new Vector3D(0, 0, 1.25);
                    }
                    else if (_open.Intersects(AnchorFaceList.ZLow))
                    {
                        _vec -= new Vector3D(0, 0, 1.25);
                    }
                }
                else
                {
                    if (_open.Intersects(AnchorFaceList.XHigh))
                    {
                        _vec -= new Vector3D(1.25, 0, 0);
                    }
                    else if (_open.Intersects(AnchorFaceList.XLow))
                    {
                        _vec += new Vector3D(1.25, 0, 0);
                    }

                    if (_open.Intersects(AnchorFaceList.YHigh))
                    {
                        _vec -= new Vector3D(0, 1.25, 0);
                    }
                    else if (_open.Intersects(AnchorFaceList.YLow))
                    {
                        _vec += new Vector3D(0, 1.25, 0);
                    }

                    if (_open.Intersects(AnchorFaceList.ZHigh))
                    {
                        _vec -= new Vector3D(0, 0, 1.25);
                    }
                    else if (_open.Intersects(AnchorFaceList.ZLow))
                    {
                        _vec += new Vector3D(0, 0, 1.25);
                    }
                }
            }
            return _vec;
        }
        #endregion

        #region public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _climbRev = _climbOpen.ReverseFace();
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
            var _travelRev = _travelOpen.ReverseFace();
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
                // setup good and bad face arrays
                var _goodFaces = AnchorFaceList.None;
                var _badFaces = AnchorFaceList.None;
                if (movement.CanMoveThrough(CellMaterial))
                {
                    _goodFaces = _goodFaces.Add(_climbRev);
                    _goodFaces = _goodFaces.Add(_travelRev);
                    _badFaces = _badFaces.Add(_climbOpen);
                    _badFaces = _badFaces.Add(_travelOpen);
                }
                else if (movement.CanMoveThrough(PlusMaterial))
                {
                    _goodFaces = _goodFaces.Add(_climbOpen);
                    _goodFaces = _goodFaces.Add(_travelOpen);
                    _badFaces = _badFaces.Add(_climbRev);
                    _badFaces = _badFaces.Add(_travelRev);
                }

                // corners (x8)
                if (AnchorFaceList.LowMask.Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 0, 0);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YLow | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 0, 0);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 5, 0);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 5, 0);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 0, 5);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 0, 5);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YHigh | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 5, 5);
                }

                if (AnchorFaceList.HighMask.Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 5, 5);
                }

                // edges (x12)
                if ((AnchorFaceList.XLow | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 2.5, 0);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 2.5, 0);
                }

                if ((AnchorFaceList.YLow | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(2.5, 0, 0);
                }

                if ((AnchorFaceList.YHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(2.5, 5, 0);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 0, 2.5);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YLow).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 0, 2.5);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 5, 2.5);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 5, 2.5);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(0, 2.5, 5);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(5, 2.5, 5);
                }

                if ((AnchorFaceList.YLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(2.5, 0, 5);
                }

                if ((AnchorFaceList.YHigh | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new Point3D(2.5, 5, 5);
                }

                // faces (x6)
                if ((_badFaces & AnchorFaceList.ZLow) == AnchorFaceList.None)
                {
                    yield return new Point3D(2.5, 2.5, 0);
                }

                if ((_badFaces & AnchorFaceList.ZHigh) == AnchorFaceList.None)
                {
                    yield return new Point3D(2.5, 2.5, 5);
                }

                if ((_badFaces & AnchorFaceList.YLow) == AnchorFaceList.None)
                {
                    yield return new Point3D(2.5, 0, 2.5);
                }

                if ((_badFaces & AnchorFaceList.YHigh) == AnchorFaceList.None)
                {
                    yield return new Point3D(2.5, 5, 2.5);
                }

                if ((_badFaces & AnchorFaceList.XLow) == AnchorFaceList.None)
                {
                    yield return new Point3D(0, 2.5, 2.5);
                }

                if ((_badFaces & AnchorFaceList.XHigh) == AnchorFaceList.None)
                {
                    yield return new Point3D(5, 2.5, 2.5);
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _climbRev = _climbOpen.ReverseFace();
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
            var _travelRev = _travelOpen.ReverseFace();
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
                // setup good and bad face arrays
                var _goodFaces = AnchorFaceList.None;
                var _badFaces = AnchorFaceList.None;
                if (movement.CanMoveThrough(CellMaterial))
                {
                    _goodFaces = _goodFaces.Add(_climbRev);
                    _goodFaces = _goodFaces.Add(_travelRev);
                    _badFaces = _badFaces.Add(_climbOpen);
                    _badFaces = _badFaces.Add(_travelOpen);
                }
                else if (movement.CanMoveThrough(PlusMaterial))
                {
                    _goodFaces = _goodFaces.Add(_climbOpen);
                    _goodFaces = _goodFaces.Add(_travelOpen);
                    _badFaces = _badFaces.Add(_climbRev);
                    _badFaces = _badFaces.Add(_travelRev);
                }

                // corners (x6)
                if ((AnchorFaceList.XLow | AnchorFaceList.YLow | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YLow | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YHigh | AnchorFaceList.ZLow).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YLow | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                }

                if ((AnchorFaceList.XLow | AnchorFaceList.YHigh | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                }

                if ((AnchorFaceList.XHigh | AnchorFaceList.YHigh | AnchorFaceList.ZHigh).Intersects(_goodFaces))
                {
                    yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                }

                // side face mid points (x2)
                var _gxl = _goodFaces.Contains(AnchorFace.XLow);
                var _gyl = _goodFaces.Contains(AnchorFace.YLow);
                var _gzl = _goodFaces.Contains(AnchorFace.ZLow);
                if (!_badFaces.Contains(AnchorFace.ZLow) && !_gzl)
                {
                    yield return new TargetCorner(new Point3D(2.5, 2.5, 0), AnchorFace.ZLow,
                        _gxl ? AnchorFace.XLow : AnchorFace.XHigh,
                        _gyl ? AnchorFace.YLow : AnchorFace.YHigh);
                }

                if (!_badFaces.Contains(AnchorFace.ZHigh) && !_goodFaces.Contains(AnchorFace.ZHigh))
                {
                    yield return new TargetCorner(new Point3D(2.5, 2.5, 5), AnchorFace.ZHigh,
                        _gxl ? AnchorFace.XLow : AnchorFace.XHigh,
                        _gyl ? AnchorFace.YLow : AnchorFace.YHigh);
                }

                if (!_badFaces.Contains(AnchorFace.YLow) && !_gyl)
                {
                    yield return new TargetCorner(new Point3D(2.5, 0, 2.5), AnchorFace.YLow,
                        _gxl ? AnchorFace.XLow : AnchorFace.XHigh,
                        _gzl ? AnchorFace.ZLow : AnchorFace.ZHigh);
                }

                if (!_badFaces.Contains(AnchorFace.YHigh) && !_goodFaces.Contains(AnchorFace.YHigh))
                {
                    yield return new TargetCorner(new Point3D(2.5, 5, 2.5), AnchorFace.YHigh,
                        _gxl ? AnchorFace.XLow : AnchorFace.XHigh,
                        _gzl ? AnchorFace.ZLow : AnchorFace.ZHigh);
                }

                if (!_badFaces.Contains(AnchorFace.XLow) && !_gxl)
                {
                    yield return new TargetCorner(new Point3D(0, 2.5, 2.5), AnchorFace.XLow,
                        _gyl ? AnchorFace.YLow : AnchorFace.YHigh,
                        _gzl ? AnchorFace.ZLow : AnchorFace.ZHigh);
                }

                if (!_badFaces.Contains(AnchorFace.XHigh) && !_goodFaces.Contains(AnchorFace.XHigh))
                {
                    yield return new TargetCorner(new Point3D(5, 2.5, 2.5), AnchorFace.XHigh,
                        _gyl ? AnchorFace.YLow : AnchorFace.YHigh,
                        _gzl ? AnchorFace.ZLow : AnchorFace.ZHigh);
                }
            }
            yield break;
        }
        #endregion

        #region public override uint FlipParameters(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(paramsIn);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(paramsIn);
            if (_climbOpen.GetAxis() == flipAxis)
            {
                return StairSpaceFaces.GetParam(_climbOpen.ReverseFace(), _travelOpen);
            }
            else if (_travelOpen.GetAxis() == flipAxis)
            {
                return StairSpaceFaces.GetParam(_climbOpen, _travelOpen.ReverseFace());
            }
            else
            {
                // flipping across the steps does nothing to them
                return paramsIn;
            }
        }
        #endregion

        #region public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            if (axis1 != axis2)
            {
                var _climbOpen = StairSpaceFaces.GetClimbOpening(paramsIn);
                var _climbAxis = _climbOpen.GetAxis();
                var _cLow = _climbOpen.IsLowFace();

                var _travelOpen = StairSpaceFaces.GetTravelOpening(paramsIn);
                var _travelAxis = _travelOpen.GetAxis();
                var _tLow = _travelOpen.IsLowFace();

                if (axis1 == _climbAxis)
                {
                    if (axis2 == _travelAxis)
                    {
                        // face swap
                        return StairSpaceFaces.GetParam(
                            _cLow ? _travelAxis.GetLowFace() : _travelAxis.GetHighFace(),
                            _tLow ? _climbAxis.GetLowFace() : _climbAxis.GetHighFace());
                    }

                    // swap climb-other
                    return StairSpaceFaces.GetParam(
                        _cLow ? axis2.GetLowFace() : axis2.GetHighFace(),
                        _travelOpen);
                }
                else if (axis1 == _travelAxis)
                {
                    if (axis2 == _climbAxis)
                    {
                        // face swap
                        return StairSpaceFaces.GetParam(
                            _cLow ? _travelAxis.GetLowFace() : _travelAxis.GetHighFace(),
                            _tLow ? _climbAxis.GetLowFace() : _climbAxis.GetHighFace());
                    }

                    // swap travel-other
                    return StairSpaceFaces.GetParam(
                        _climbOpen,
                        _tLow ? axis2.GetLowFace() : axis2.GetHighFace());
                }
                else
                {
                    if (axis2 == _climbAxis)
                    {
                        // swap climb-other
                        return StairSpaceFaces.GetParam(
                            _cLow ? axis1.GetLowFace() : axis1.GetHighFace(),
                            _travelOpen);
                    }

                    // swap climb-other
                    return StairSpaceFaces.GetParam(
                        _climbOpen,
                        _tLow ? axis1.GetLowFace() : axis1.GetHighFace());
                }
            }
            return paramsIn;
        }
        #endregion

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => StairSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => StairSpaceFaces.ShowFace(param, this, outwardFace);

        public override CellSpaceInfo ToCellSpaceInfo()
            => new StairSpaceInfo(this);

        #region public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock ^ _plusBlock)
            {
                // only 1 can block to get inner grip structure
                var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
                var _travelOpen = StairSpaceFaces.GetTravelOpening(param);
                var _gravAxis = gravity.GetAxis();
                var _faces = InnerGripBaseFace(_cellBlock, _plusBlock, _climbOpen, _travelOpen, gravity);

                if (_gravAxis != _climbOpen.GetAxis() && _gravAxis != _travelOpen.GetAxis())
                {
                    // gravity is not with the stair run in any way
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerBase(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
                else if ((_cellBlock && (gravity == _climbOpen || gravity == _travelOpen))
                    || (_plusBlock && (gravity == _climbOpen.ReverseFace() || gravity == _travelOpen.ReverseFace())))
                {
                    // gravity points to an open side
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerDangling(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
                else
                {
                    // inner ledge (handled by the base?)
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerLedge(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
            }

            // no ledge
            return new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
        }
        #endregion

        #region private AnchorFaceList InnerGripBaseFace(bool cellBlock, bool plusBlock, AnchorFace climbOpen, AnchorFace travelOpen, AnchorFace gravity)
        private AnchorFaceList InnerGripBaseFace(bool cellBlock, bool plusBlock,
            AnchorFace climbOpen, AnchorFace travelOpen, AnchorFace gravity)
        {
            if (cellBlock ^ plusBlock)
            {
                // only 1 can block to get inner grip structure
                var _gravAxis = gravity.GetAxis();
                if (_gravAxis != climbOpen.GetAxis() && _gravAxis != travelOpen.GetAxis())
                {
                    return (cellBlock
                        ? climbOpen
                        : climbOpen.ReverseFace()).ToAnchorFaceList();
                }
                else if (cellBlock)
                {
                    return (_gravAxis == climbOpen.GetAxis()
                        ? travelOpen
                        : climbOpen).ToAnchorFaceList();
                }
                else
                {
                    return (_gravAxis == climbOpen.GetAxis()
                        ? travelOpen.ReverseFace()
                        : climbOpen.ReverseFace()).ToAnchorFaceList();
                }
            }

            // default
            return AnchorFaceList.None;
        }
        #endregion

        #region public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            // cell materials are on climb open and travel open
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);

            // disposition
            var _gripType =
                ((gripFace == _climbOpen) || (gripFace == _travelOpen))
                ? new { Disposition = GripDisposition.Full, IsPlus = (bool?)false }
                : ((gripFace == _climbOpen.ReverseFace()) || (gripFace == _travelOpen.ReverseFace()))
                ? new { Disposition = GripDisposition.Full, IsPlus = (bool?)true }
                : new { Disposition = GripDisposition.Irregular, IsPlus = (bool?)null };

            if (gripFace == gravity)
            {
                // dangling...
                if (_gripType.Disposition == GripDisposition.Full)
                {
                    return GripRules.GetOuterDangling((bool)_gripType.IsPlus);
                }

                return GripRules.GetOuterDangling(_gripType.Disposition);
            }

            // full face material
            if (_gripType.Disposition == GripDisposition.Full)
            {
                return GripRules.GetOuterBase((bool)_gripType.IsPlus);
            }

            // ledges on the irregular face?
            // ... must have only one material blocking, and oriented correctly
            var _cellBlocks = !movement.CanMoveThrough(CellMaterial);
            var _plusBlocks = !movement.CanMoveThrough(PlusMaterial);
            if ((_cellBlocks ^ _plusBlocks) &&
                (
                    (((gravity == _climbOpen) || (gravity == _travelOpen)) && _cellBlocks)
                    ||
                    (((gravity == _climbOpen.ReverseFace()) || (gravity == _travelOpen.ReverseFace())) && _plusBlocks)
                )
               )
            {
                // ledge
                return GripRules.GetOuterLedge(_plusBlocks, _gripType.Disposition);
            }

            // irregular face (no ledges)
            return GripRules.GetOuterBase(_gripType.Disposition);
        }
        #endregion

        public override int? InnerSwimDifficulty(uint param)
            => (new[] { base.InnerSwimDifficulty(param), MaterialSwimDifficulty(PlusMaterial) }).Max();
    }
}
