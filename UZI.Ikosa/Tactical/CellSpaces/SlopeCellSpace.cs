using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SlopeCellSpace : SliverCellSpace
    {
        #region construction
        public SlopeCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles)
            : this(minusMaterial, minusTiles, plusMaterial, plusTiles, true)
        {
        }

        protected SlopeCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles,
            bool defer)
            : base(minusMaterial, minusTiles, plusMaterial, plusTiles)
        {
        }
        #endregion

        public override string GetDescription(uint param)
            => $@"Slp:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName})";

        public override string GetParamText(uint param)
        {
            var _param = new SliverSlopeParams(param);
            return $@"Face={(_param.Flip ? _param.Axis.GetHighFace() : _param.Axis.GetLowFace())}, Slope={_param.SlopeAxis}, Lo={_param.Offset}, Hi={_param.HiOffset}";
        }

        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            SlopeSpaceFaces.AddInnerStructures(new SliverSlopeParams(param), this, CellEdge, addToGroup, z, y, x, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            SlopeSpaceFaces.AddOuterSurface(new SliverSlopeParams(param), this, CellEdge, group, z, y, x, face, effect, bump);
        }

        #region bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            // upper vector and material
            var _param = new SliverSlopeParams(param);
            if (UpperMaterial(_param).BlocksEffect)
            {
                if (AnyUpperVector(_param, z, y, x, pt1, pt2))
                {
                    return true;
                }
            }

            // lower vector and material
            if (LowerMaterial(_param).BlocksEffect)
            {
                if (AnyLowerVector(_param, z, y, x, pt1, pt2))
                {
                    return true;
                }
            }

            // nothing blocking the effect
            return false;
        }
        #endregion

        protected bool AnyUpperVector(SliverSlopeParams param, int z, int y, int x, Point3D p1, Point3D p2)
        {
            return (from _plane in UpperPlanes(param, z, y, x, p1)
                    where _plane.SegmentIntersection(p1, p2).HasValue
                    select _plane).Any();
        }

        protected bool AnyLowerVector(SliverSlopeParams param, int z, int y, int x, Point3D p1, Point3D p2)
        {
            return (from _plane in LowerPlanes(param, z, y, x, p1)
                    where _plane.SegmentIntersection(p1, p2).HasValue
                    select _plane).Any();
        }

        protected override IEnumerable<PlanarPoints> UpperPlanes(SliverSlopeParams param, int z, int y, int x, Point3D facingPoint)
            => SlopeSpaceFaces.GeneratePlanes(param, z, y, x, true, facingPoint);

        protected override IEnumerable<PlanarPoints> LowerPlanes(SliverSlopeParams param, int z, int y, int x, Point3D facingPoint)
            => SlopeSpaceFaces.GeneratePlanes(param, z, y, x, false, facingPoint);

        #region public override uint FlipParameters(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            // decompose to individual parameters
            var _param = new SliverSlopeParams(paramsIn);

            // see if any need to flip
            if (_param.Axis == flipAxis)
            {
                _param.Flip = !_param.Flip;
            }

            if (_param.SlopeAxis == flipAxis)
            {
                var _hi = _param.HiOffsetUnits;
                var _lo = _param.OffsetUnits;
                _param.HiOffsetUnits = _lo;
                _param.OffsetUnits = _hi;
            }


            // recompose to uint parameter
            return _param.Value;
        }
        #endregion

        #region public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            if (axis1 != axis2)
            {
                // decompose to individual parameters
                var _param = new SliverSlopeParams(paramsIn);
                var _axis = _param.Axis;
                var _flip = _param.Flip;
                var _slopeAxis = _param.SlopeAxis;

                if ((axis1 == _axis))
                {
                    if (axis2 == _slopeAxis)
                    {
                        // swapping slopeAxis and mainAxis
                        var _tmp = _axis;
                        _param.Axis = _slopeAxis;
                        _param.SlopeAxis = _tmp;
                        return _param.Value;
                    }

                    // swapping mainAxis
                    _param.Axis = axis2;
                    return _param.Value;
                }
                else if (axis2 == _axis)
                {
                    if (axis1 == _slopeAxis)
                    {
                        // swapping slopeAxis and mainAxis
                        var _tmp = _axis;
                        _param.Axis = _slopeAxis;
                        _param.SlopeAxis = _tmp;
                        return _param.Value;
                    }

                    // swapping mainAxis
                    _param.Axis = axis1;
                    return _param.Value;
                }
                else if (axis1 == _slopeAxis)
                {
                    // just swapping slope axis
                    _param.SlopeAxis = axis2;
                    return _param.Value;
                }
                else if (axis2 == _slopeAxis)
                {
                    // just swapping slope axis
                    _param.SlopeAxis = axis1;
                    return _param.Value;
                }
            }
            return paramsIn;
        }
        #endregion

        #region public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            // get paramameters
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _fOffset = _param.Offset;
            var _pfOffset = _param.HiOffset;
            var _ortho = _param.Axis;
            var _slopeAxis = _param.SlopeAxis;

            if (surfaceFace.IsOrthogonalTo(_ortho))
            {
                // trying to cross in the same axis as the sliver
                if (surfaceFace.IsLowFace())
                {
                    return new HedralGrip(!movement.CanMoveThrough(_lower));
                }
                else
                {
                    return new HedralGrip(!movement.CanMoveThrough(_upper));
                }
            }
            else if (IsShell)
            {
                // ignore calculated blocking
                return new HedralGrip(false);
            }
            else
            {
                if (movement.CanMoveThrough(_lower))
                {
                    // can move through cell material
                    if (movement.CanMoveThrough(_upper))
                    {
                        // ... and plus material
                        return new HedralGrip(false);
                    }

                    // could only move through cell material (plus blocks)
                    if (surfaceFace.IsOrthogonalTo(_slopeAxis))
                    {
                        return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetHighFace(),
                            (surfaceFace.IsLowFace() ? _fOffset : _pfOffset));
                    }
                    else
                    {
                        // average of offsets
                        return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetHighFace(), _fOffset, _pfOffset);
                    }
                }
                else if (movement.CanMoveThrough(_upper))
                {
                    // could only move through plus material (cell blocks)
                    if (surfaceFace.IsOrthogonalTo(_slopeAxis))
                    {
                        return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetLowFace(),
                            (surfaceFace.IsLowFace() ? _fOffset : _pfOffset));
                    }
                    else
                    {
                        // average of offsets
                        return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetLowFace(), _fOffset, _pfOffset);
                    }
                }

                // could not move through either material
                return new HedralGrip(true);
            }
        }
        #endregion

        // TODO:

        #region public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // cannot move through both
            if (!ValidSpace(param, movement))
            {
                // params
                var _param = new SliverSlopeParams(param);
                var _upper = UpperMaterial(_param);
                var _lower = LowerMaterial(_param);

                // both low and high cannot be blocked
                var _lowBlocked = !movement.CanMoveThrough(_lower);
                var _highBlocked = !movement.CanMoveThrough(_upper);
                if (!(_lowBlocked && _highBlocked))
                {
                    MovementOpening _orthopening(AnchorFace face, double offset)
                    {
                        // ortho opening
                        return new MovementOpening(face, offset, 1);
                    };
                    MovementOpening _slopening(AnchorFace face, double offset, double amount)
                    {
                        // slope opening
                        return new MovementOpening(face, offset, amount / 5d);
                    };

                    // params
                    var _loOff = _param.LoFlippableOffset;
                    var _hiOff = _param.HiFlippableOffset;
                    var _mOffset = (_loOff + _hiOff) / 2;
                    var _ortho = _param.Axis;
                    var _slope = _param.SlopeAxis;
                    if (_lowBlocked)
                    {
                        // lo blocked (so the difference is the amount of empty space)
                        yield return _orthopening(_ortho.GetHighFace(), 5d - _mOffset);
                        if (_ortho != baseFace.GetAxis())
                        {
                            yield return _slopening((_loOff < _hiOff) ? _slope.GetLowFace() : _slope.GetHighFace(), 2.5d, _mOffset);
                        }
                    }
                    else
                    {
                        // hi blocked (so the offset is the amount of empty space)
                        yield return _orthopening(_ortho.GetLowFace(), _mOffset);
                        if (_ortho != baseFace.GetAxis())
                        {
                            yield return _slopening((_loOff > _hiOff) ? _slope.GetLowFace() : _slope.GetHighFace(), 2.5d, 5d - _mOffset);
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _ortho = _param.Axis;
            var _slope = _param.SlopeAxis;
            var _loOffset = _param.LoFlippableOffset;
            var _hiOffset = _param.HiFlippableOffset;

            // cannot move through both
            if (!ValidSpace(param, movement))
            {
                var _loBlocked = !movement.CanMoveThrough(_lower);
                var _hiBlocked = !movement.CanMoveThrough(_upper);
                var _lowFace = upDirection.IsLowFace();

                // moving across the blocked axis?
                if (upDirection.GetAxis() == _ortho)
                {
                    if (_lowFace ^ _loBlocked)
                    {
                        // opposite face blocking, so slope has elevations
                        yield return new SlopeSegment
                        {
                            Low = Math.Min(_loOffset, _hiOffset) + baseElev,
                            High = Math.Max(_loOffset, _hiOffset) + baseElev,
                            Run = 5
                        };
                    }
                }
                else
                {
                    // moving across the sloped axis?
                    if (upDirection.GetAxis() == _slope)
                    {
                        // slope is exposed to movement direction?
                        if ((_lowFace && ((_loBlocked && (_loOffset < _hiOffset)) || (_hiBlocked && (_loOffset > _hiOffset))))
                            || (!_lowFace && ((_loBlocked && (_loOffset > _hiOffset)) || (_hiBlocked && (_loOffset < _hiOffset)))))
                        {
                            yield return new SlopeSegment
                            {
                                Low = baseElev,
                                High = 5 + baseElev,
                                Run = Math.Abs(_hiOffset - _loOffset)
                            };
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        {
            // cannot move through both
            if (!ValidSpace(param, movement))
            {
                // params
                var _param = new SliverSlopeParams(param);
                var _upper = UpperMaterial(_param);
                var _lower = LowerMaterial(_param);

                // both low and high cannot be blocked
                var _lowBlocked = !movement.CanMoveThrough(_lower);
                var _highBlocked = !movement.CanMoveThrough(_upper);
                if (!(_lowBlocked && _highBlocked))
                {
                    // param
                    var _loOff = _param.LoFlippableOffset;
                    var _hiOff = _param.HiFlippableOffset;
                    var _mOffset = (_loOff + _hiOff) / 2;
                    var _ortho = _param.Axis;
                    var _slope = _param.SlopeAxis;

                    #region ordinate function
                    double _ord(Axis axis)
                    {
                        if (axis == _ortho)
                        {
                            return _highBlocked ? _mOffset - 5 : _mOffset;
                        }
                        else
                        {
                            var _rise = Math.Abs(_loOff - _hiOff);
                            if ((axis == _slope) && (_rise > 2.5))
                            {
                                if (_highBlocked)
                                {
                                    // positive
                                    return (_loOff < _hiOff)
                                    ? Math.Max(_rise - 2.5d, 2.5d)          // positive
                                    : 0d - Math.Max(_rise - 2.5d, 2.5d);    // negative
                                }

                                return (_loOff < _hiOff)
                                ? 0d - Math.Max(_rise - 2.5d, 2.5d) // negative
                                : Math.Max(_rise - 2.5d, 2.5d);     // positive
                            }
                        }
                        return 0d;
                    };
                    #endregion

                    return new Vector3D(_ord(Axis.X), _ord(Axis.Y), _ord(Axis.Z));
                }
            }
            return base.OrthoOffset(param, movement, gravity);
        }
        #endregion

        #region public static IEnumerable<Point3D> GetTacticalPoints(bool loBlock, Axis ortho, Axis slope, double loOffset, double hiOffset)
        /// <summary>Yields all points for a slope-like structure</summary>
        /// <param name="loBlock">true if blocking face is the low face</param>
        /// <param name="ortho">axis for the blocking face</param>
        /// <param name="slope">axis over which the slope runs</param>
        /// <param name="loOffset">low face offset from 0</param>
        /// <param name="hiOffset">high face offset from 0</param>
        public static IEnumerable<Point3D> GetTacticalPoints(bool loBlock, Axis ortho, Axis slope, double loOffset, double hiOffset)
        {
            // burn to matrix
            var _matrix = SlopeSpaceFaces.GetOrthoAndSlopeTransform(ortho, slope);

            if (loBlock)
            {
                // corners
                yield return _matrix.Transform(new Point3D(0, 0, loOffset));
                yield return _matrix.Transform(new Point3D(5, 0, hiOffset));
                yield return _matrix.Transform(new Point3D(0, 5, loOffset));
                yield return _matrix.Transform(new Point3D(5, 5, hiOffset));
                yield return _matrix.Transform(new Point3D(0, 0, 5));
                yield return _matrix.Transform(new Point3D(5, 0, 5));
                yield return _matrix.Transform(new Point3D(0, 5, 5));
                yield return _matrix.Transform(new Point3D(5, 5, 5));

                if (loOffset <= 2.5)
                {
                    var _loCenter = (5 + loOffset) / 2;
                    // side-edges
                    yield return _matrix.Transform(new Point3D(0, 0, _loCenter));
                    yield return _matrix.Transform(new Point3D(0, 5, _loCenter));
                    // low face
                    yield return _matrix.Transform(new Point3D(0, 2.5, _loCenter));
                    // bottom edge
                    yield return _matrix.Transform(new Point3D(0, 2.5, loOffset));
                }
                if (hiOffset <= 2.5)
                {
                    var _hiCenter = (5 + hiOffset) / 2;
                    // side-edges
                    yield return _matrix.Transform(new Point3D(5, 0, _hiCenter));
                    yield return _matrix.Transform(new Point3D(5, 5, _hiCenter));
                    // high face
                    yield return _matrix.Transform(new Point3D(5, 2.5, _hiCenter));
                    // bottom edge
                    yield return _matrix.Transform(new Point3D(5, 2.5, hiOffset));
                }

                // top-edges
                yield return _matrix.Transform(new Point3D(0, 2.5, 5));
                yield return _matrix.Transform(new Point3D(5, 2.5, 5));
                yield return _matrix.Transform(new Point3D(2.5, 0, 5));
                yield return _matrix.Transform(new Point3D(2.5, 5, 5));

                var _midOffset = (loOffset + hiOffset) / 2;
                if (_midOffset <= 2.5)
                {
                    // bottom-edges
                    yield return _matrix.Transform(new Point3D(2.5, 0, _midOffset));
                    yield return _matrix.Transform(new Point3D(2.5, 5, _midOffset));
                    // bottom face
                    yield return _matrix.Transform(new Point3D(2.5, 2.5, _midOffset));

                    var _midCenter = (5 + _midOffset) / 2;

                    // "side" faces
                    yield return _matrix.Transform(new Point3D(2.5, 0, _midCenter));
                    yield return _matrix.Transform(new Point3D(2.5, 5, _midCenter));
                    // pure center
                    yield return _matrix.Transform(new Point3D(2.5, 2.5, _midCenter));
                }

                // top face
                yield return _matrix.Transform(new Point3D(2.5, 2.5, 5));
            }
            else
            {
                // corners
                yield return _matrix.Transform(new Point3D(0, 0, 0));
                yield return _matrix.Transform(new Point3D(5, 0, 0));
                yield return _matrix.Transform(new Point3D(0, 5, 0));
                yield return _matrix.Transform(new Point3D(5, 5, 0));
                yield return _matrix.Transform(new Point3D(0, 0, loOffset));
                yield return _matrix.Transform(new Point3D(0, 5, loOffset));
                yield return _matrix.Transform(new Point3D(5, 0, hiOffset));
                yield return _matrix.Transform(new Point3D(5, 5, hiOffset));

                if (loOffset >= 2.5)
                {
                    var _loCenter = loOffset / 2;
                    // side-edges
                    yield return _matrix.Transform(new Point3D(0, 0, _loCenter));
                    yield return _matrix.Transform(new Point3D(0, 5, _loCenter));
                    // low face
                    yield return _matrix.Transform(new Point3D(0, 2.5, _loCenter));
                    // top edge
                    yield return _matrix.Transform(new Point3D(0, 2.5, loOffset));
                }
                if (hiOffset >= 2.5)
                {
                    var _hiCenter = hiOffset / 2;
                    // side-edges
                    yield return _matrix.Transform(new Point3D(5, 0, _hiCenter));
                    yield return _matrix.Transform(new Point3D(5, 5, _hiCenter));
                    // high face
                    yield return _matrix.Transform(new Point3D(5, 2.5, _hiCenter));
                    // top edge
                    yield return _matrix.Transform(new Point3D(5, 2.5, hiOffset));
                }

                // bottom-edges
                yield return _matrix.Transform(new Point3D(0, 2.5, 0));
                yield return _matrix.Transform(new Point3D(5, 2.5, 0));
                yield return _matrix.Transform(new Point3D(2.5, 0, 0));
                yield return _matrix.Transform(new Point3D(2.5, 5, 0));

                var _midOffset = (loOffset + hiOffset) / 2;
                if (_midOffset >= 2.5)
                {
                    // top-edges
                    yield return _matrix.Transform(new Point3D(2.5, 0, _midOffset));
                    yield return _matrix.Transform(new Point3D(2.5, 5, _midOffset));
                    // top face
                    yield return _matrix.Transform(new Point3D(2.5, 2.5, _midOffset));

                    var _midCenter = _midOffset / 2;

                    // "side" faces
                    yield return _matrix.Transform(new Point3D(2.5, 0, _midCenter));
                    yield return _matrix.Transform(new Point3D(2.5, 5, _midCenter));
                    // pure center
                    yield return _matrix.Transform(new Point3D(2.5, 2.5, _midCenter));
                }

                // bottom face
                yield return _matrix.Transform(new Point3D(2.5, 2.5, 0));
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlock = !movement.CanMoveThrough(_lower);
            var _hiBlock = !movement.CanMoveThrough(_upper);
            if (_loBlock && _hiBlock)
            {
                // no points
            }
            else if (!_loBlock && !_hiBlock)
            {
                // same as base
                foreach (var _pt in base.TacticalPoints(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                // critical values
                var _loOffset = _param.LoFlippableOffset;
                var _hiOffset = _param.HiFlippableOffset;
                var _ortho = _param.Axis;
                var _slope = _param.SlopeAxis;

                foreach (var _pt in GetTacticalPoints(_loBlock, _ortho, _slope, _loOffset, _hiOffset))
                {
                    yield return _pt;
                }
            }
            yield break;
        }
        #endregion

        #region public override Vector3D InteractionOffset3D(uint param)
        public override Vector3D InteractionOffset3D(uint param)
        {
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlock = _lower.BlocksEffect;
            var _hiBlock = _upper.BlocksEffect;
            var _displacement = ((_param.LoFlippableOffset + _param.HiFlippableOffset) / 4) - (_loBlock ? 0d : 2.5d);
            var _ortho = _param.Axis;
            if (_loBlock ^ _hiBlock)
            {
                switch (_ortho)
                {
                    case Axis.X:
                        return new Vector3D(_displacement, 0, 0);
                    case Axis.Y:
                        return new Vector3D(0, _displacement, 0);
                    case Axis.Z:
                        return new Vector3D(0, 0, _displacement);
                }
            }
            return base.InteractionOffset3D(param);
        }
        #endregion

        #region public static IEnumerable<TargetCorner> GetTargetCorners(Axis ortho, Axis slope, bool loBlock, double loOffset, double hiOffset)
        public static IEnumerable<TargetCorner> GetTargetCorners(Axis ortho, Axis slope, bool loBlock, double loOffset, double hiOffset)
        {
            switch (ortho)
            {
                case Axis.Z:
                    if (loBlock)
                    {
                        #region upper Z
                        if (slope == Axis.X)
                        {
                            // start at slope (vary by X)
                            yield return new TargetCorner(new Point3D(0, 0, loOffset), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, hiOffset), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, loOffset), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, hiOffset), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                            // go to top
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at slope (vary by Y)
                            yield return new TargetCorner(new Point3D(0, 0, loOffset), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, hiOffset), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, loOffset), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, hiOffset), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                            // go to top
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    else
                    {
                        #region lower Z
                        // hi block
                        if (slope == Axis.X)
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                            // go to slope (vary by X)
                            yield return new TargetCorner(new Point3D(0, 0, loOffset), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, hiOffset), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, loOffset), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, hiOffset), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                            // go to slope (vary by Y)
                            yield return new TargetCorner(new Point3D(0, 0, loOffset), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, hiOffset), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, loOffset), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, hiOffset), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    break;

                case Axis.Y:
                    if (loBlock)
                    {
                        #region Upper Y
                        if (slope == Axis.X)
                        {
                            // start at slope (vary by X)
                            yield return new TargetCorner(new Point3D(0, loOffset, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, loOffset, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);

                            // go to top
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at slope (vary by Z)
                            yield return new TargetCorner(new Point3D(0, loOffset, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, hiOffset, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, loOffset, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);

                            // go to top
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    else
                    {
                        #region lower Y
                        // hi block
                        if (slope == Axis.X)
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);

                            // go to slope (vary by X)
                            yield return new TargetCorner(new Point3D(0, loOffset, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, loOffset, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);

                            // go to slope (vary by Z)
                            yield return new TargetCorner(new Point3D(0, loOffset, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, hiOffset, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, loOffset, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, hiOffset, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    break;

                case Axis.X:
                default:
                    if (loBlock)
                    {
                        #region upper X
                        if (slope == Axis.X)
                        {
                            // start at slope (vary by Y)
                            yield return new TargetCorner(new Point3D(loOffset, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(loOffset, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);

                            // go to top
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at slope (vary by Z)
                            yield return new TargetCorner(new Point3D(loOffset, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(loOffset, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);

                            // go to top
                            yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    else
                    {
                        #region lower X
                        // hi block
                        if (slope == Axis.X)
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);

                            // go to slope (vary by Y)
                            yield return new TargetCorner(new Point3D(loOffset, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(loOffset, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        else
                        {
                            // start at bottom
                            yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);

                            // go to slope (vary by Z)
                            yield return new TargetCorner(new Point3D(loOffset, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                            yield return new TargetCorner(new Point3D(loOffset, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                            yield return new TargetCorner(new Point3D(hiOffset, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                        }
                        #endregion
                    }
                    break;
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlock = !movement.CanMoveThrough(_lower);
            var _hiBlock = !movement.CanMoveThrough(_upper);
            if (_loBlock && _hiBlock)
            {
                // no points
            }
            else if (!_loBlock && !_hiBlock)
            {
                // same as base
                foreach (var _pt in base.TargetCorners(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                // critical values
                var _loOffset = _param.LoFlippableOffset;
                var _hiOffset = _param.HiFlippableOffset;
                var _ortho = _param.Axis;
                var _slope = _param.SlopeAxis;
                foreach (var _corner in GetTargetCorners(_ortho, _slope, _loBlock, _loOffset, _hiOffset))
                {
                    yield return _corner;
                }
            }
            yield break;
        }
        #endregion

        public override CellSpaceInfo ToCellSpaceInfo()
            => new SlopeSpaceInfo(this, CellEdge);

        #region public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlocked = !movement.CanMoveThrough(_lower);
            var _hiBlocked = !movement.CanMoveThrough(_upper);
            var _loOffset = _param.LoFlippableOffset;
            var _hiOffset = _param.HiFlippableOffset;
            var _plusBlocked = !movement.CanMoveThrough(PlusMaterial);

            // only 1 material must block for inner climb
            if (_loBlocked ^ _hiBlocked)
            {
                var _gAxis = gravity.GetAxis();
                var _ortho = _param.Axis;
                var _faces = _loBlocked
                    ? _ortho.GetLowFace().ToAnchorFaceList()
                    : _ortho.GetHighFace().ToAnchorFaceList();
                var _slope = _param.SlopeAxis;

                if (_gAxis != _ortho && _gAxis != _slope)
                {
                    // gravity is neither axis...
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerBase(_plusBlocked),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
                else if (_gAxis == _ortho)
                {
                    // either dangling or standing on a slope between 0°-45°
                    var _gravityLow = gravity.IsLowFace();
                    if ((_loBlocked && !_gravityLow) || (_hiBlocked && _gravityLow))
                    {
                        // dangling on the inner face
                        return new CellGripResult
                        {
                            Difficulty = GripRules.GetInnerDangling(_plusBlocked),
                            Faces = _faces,
                            InnerFaces = _faces
                        };
                    }

                    // slighter slope
                    var _segment =
                        new SlopeSegment
                        {
                            Low = 0,
                            High = 5,
                            Run = Math.Abs(_hiOffset - _loOffset)
                        };
                    if (_segment.Incline() > 30)
                    {
                        // slope is a slope
                        return new CellGripResult
                        {
                            Difficulty = 0,
                            Faces = _faces,
                            InnerFaces = _faces
                        };
                    }

                    // slope is trivial
                    return new CellGripResult
                    {
                        Difficulty = -10,
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
                else if (_gAxis == _slope)
                {
                    // gravity is slope axis...45°-90°
                    var _gravLow = gravity.IsLowFace();

                    // slope is exposed to movement direction?
                    if ((!_gravLow && ((_loBlocked && (_loOffset < _hiOffset)) || (_hiBlocked && (_loOffset > _hiOffset))))
                        || (_gravLow && ((_loBlocked && (_loOffset > _hiOffset)) || (_hiBlocked && (_loOffset < _hiOffset)))))
                    {
                        var _segment =
                            new SlopeSegment
                            {
                                Low = 0,
                                High = 5,
                                Run = Math.Abs(_hiOffset - _loOffset)
                            };
                        if (_segment.Incline() > 60)
                        {
                            // slope is a wall
                            return new CellGripResult
                            {
                                Difficulty = GripRules.GetInnerBase(_plusBlocked),
                                Faces = _faces,
                                InnerFaces = _faces
                            };
                        }

                        // just a slope
                        return new CellGripResult
                        {
                            Difficulty = 0,
                            Faces = _faces,
                            InnerFaces = _faces
                        };
                    }

                    // if slope runs "outwards", then use dangling
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerDangling(_plusBlocked),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
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

        /// <summary>Overrides GetGripDisposition to provide Irregular for slope sides</summary>
        protected override GripDisposition GetGripDisposition(SliverSlopeParams param, AnchorFace gripFace, Axis axis)
            => axis == gripFace.GetAxis()
            ? GripDisposition.Full
            : param.SlopeAxis == gripFace.GetAxis()
            ? GripDisposition.Rectangular
            : GripDisposition.Irregular;
    }
}
