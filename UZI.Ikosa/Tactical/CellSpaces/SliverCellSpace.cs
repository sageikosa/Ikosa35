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
    public class SliverCellSpace : CellSpace, IPlusCellSpace // TODO: INotifyPropertyChanged
    {
        #region Construction
        public SliverCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles)
            : this(minusMaterial, minusTiles, plusMaterial, plusTiles, true)
        {
            GripRules.InitializeMaterials(minusMaterial, plusMaterial);
        }

        protected SliverCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles,
            bool defer)
            : base(minusMaterial, minusTiles, defer)
        {
            _PlusMaterial = plusMaterial;
            _PlusTiling = plusTiles;
            _Edge = new CellEdge();
        }

        static SliverCellSpace()
        {
            _XLN = AnchorFace.XLow.GetNormalVector();
            _XHN = AnchorFace.XHigh.GetNormalVector();
            _YLN = AnchorFace.YLow.GetNormalVector();
            _YHN = AnchorFace.YHigh.GetNormalVector();
            _ZLN = AnchorFace.ZLow.GetNormalVector();
            _ZHN = AnchorFace.ZHigh.GetNormalVector();
        }
        #endregion

        #region private data
        private CellMaterial _PlusMaterial;
        private TileSet _PlusTiling;
        private bool _Shell = false;
        private CellEdge _Edge = new CellEdge();

        private static Vector3D _XLN;
        private static Vector3D _XHN;
        private static Vector3D _YLN;
        private static Vector3D _YHN;
        private static Vector3D _ZLN;
        private static Vector3D _ZHN;
        #endregion

        #region public CellMaterial PlusMaterial { get; set; }
        public CellMaterial PlusMaterial
        {
            get { return _PlusMaterial; }
            set
            {
                if (_PlusMaterial != value)
                {
                    _PlusMaterial = value;
                    OnCellMaterialChanged();
                    DoPropertyChanged(nameof(PlusMaterial));
                }
            }
        }

        protected override void OnCellMaterialChanged()
        {
            GripRules.InitializeMaterials(CellMaterial, PlusMaterial);
        }
        #endregion

        #region public TileSet PlusTiling { get; set; }
        public TileSet PlusTiling
        {
            get { return _PlusTiling; }
            set
            {
                if (_PlusTiling != value)
                {
                    _PlusTiling = value;
                    DoPropertyChanged(nameof(PlusTiling));
                }
            }
        }
        #endregion

        public override IEnumerable<CellMaterial> AllMaterials
        {
            get
            {
                yield return CellMaterial;
                yield return PlusMaterial;
                yield break;
            }
        }

        public override (string collectionKey, string brushKey) InnerBrushKeys(uint param, Point3D point)
            => (IsPlusGas || IsPlusLiquid)
            ? (_PlusTiling?.BrushCollectionKey, _PlusTiling?.BrushCollection[_PlusTiling.InnerMaterialIndex].BrushKey)
            : base.InnerBrushKeys(param, point);

        public override string GetDescription(uint param)
            => $@"Slv:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName})";

        public override string GetParamText(uint param)
        {
            var _param = new SliverSlopeParams(param);
            return $@"Face={(_param.Flip ? _param.Axis.GetHighFace() : _param.Axis.GetLowFace())}, Ofs={_param.Offset}";
        }

        public CellEdge CellEdge
            => _Edge ??= new CellEdge();

        #region protected virtual IEnumerable<PlanarPoints> UpperPlanes(SliverSlopeParams param, int z, int y, int x)
        /// <summary>Used to find intersections with the upper material</summary>
        protected virtual IEnumerable<PlanarPoints> UpperPlanes(SliverSlopeParams param, int z, int y, int x, Point3D facingPt)
        {
            var _fOffset = param.LoFlippableOffset;
            var _ortho = param.Axis;

            var _xL = x * 5d;
            var _yL = y * 5d;
            var _zL = z * 5d;
            var _xH = (x + 1) * 5d;
            var _yH = (y + 1) * 5d;
            var _zH = (z + 1) * 5d;
            switch (_ortho)
            {
                case Axis.X: _xL += _fOffset; break;
                case Axis.Y: _yL += _fOffset; break;
                default: _zL += _fOffset; break;
            }

            // x planes
            if (facingPt.X < _xL)
            {
                yield return new PlanarPoints(_XLN, new Point3D(_xL, _yL, _zL), new Point3D(_xL, _yL, _zH), new Point3D(_xL, _yH, _zH), new Point3D(_xL, _yH, _zL));
            }

            if (facingPt.X > _xH)
            {
                yield return new PlanarPoints(_XHN, new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yL, _zH), new Point3D(_xH, _yH, _zH), new Point3D(_xH, _yH, _zL));
            }
            // y planes
            if (facingPt.Y < _yL)
            {
                yield return new PlanarPoints(_YLN, new Point3D(_xL, _yL, _zL), new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yL, _zH), new Point3D(_xL, _yL, _zH));
            }

            if (facingPt.Y > _yH)
            {
                yield return new PlanarPoints(_YHN, new Point3D(_xL, _yH, _zL), new Point3D(_xH, _yH, _zL), new Point3D(_xH, _yH, _zH), new Point3D(_xL, _yH, _zH));
            }
            // z planes
            if (facingPt.Z < _zL)
            {
                yield return new PlanarPoints(_ZLN, new Point3D(_xL, _yL, _zL), new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yH, _zL), new Point3D(_xL, _yH, _zL));
            }

            if (facingPt.Z > _zH)
            {
                yield return new PlanarPoints(_ZHN, new Point3D(_xL, _yL, _zH), new Point3D(_xH, _yL, _zH), new Point3D(_xH, _yH, _zH), new Point3D(_xL, _yH, _zH));
            }

            yield break;
        }
        #endregion

        #region protected Vector3D? UpperVector(uint param, int z, int y, int x, Point3D p1, Point3D p2)
        protected Vector3D? UpperVector(SliverSlopeParams param, int z, int y, int x, Point3D p1, Point3D p2)
        {
            // all intersecting points
            var _points = (from _plane in UpperPlanes(param, z, y, x, p1)
                           let _pt = _plane.SegmentIntersection(p1, p2)
                           where _pt.HasValue
                           select _pt.Value).Distinct().ToList();
            if (_points.Count > 1)
            {
                // get longest vector
                return (from _p in _points
                        from _r in _points
                        select _p - _r).OrderByDescending(_v => _v.LengthSquared).First();
            }

            // none
            return null;
        }
        #endregion

        #region protected virtual IEnumerable<PlanarPoints> LowerPlanes(SliverSlopeParams param, int z, int y, int x)
        /// <summary>Used to find intersections with the lower material</summary>
        protected virtual IEnumerable<PlanarPoints> LowerPlanes(SliverSlopeParams param, int z, int y, int x, Point3D facingPt)
        {
            var _fOffset = param.LoFlippableOffset;
            var _ortho = param.Axis;

            var _xL = x * 5d;
            var _yL = y * 5d;
            var _zL = z * 5d;
            var _xH = (x + 1) * 5d;
            var _yH = (y + 1) * 5d;
            var _zH = (z + 1) * 5d;
            switch (_ortho)
            {
                case Axis.X: _xH = (x * 5d) + _fOffset; break;
                case Axis.Y: _yH = (y * 5d) + _fOffset; break;
                default: _zH = (z * 5d) + _fOffset; break;
            }

            // x planes
            if (facingPt.X < _xL)
            {
                yield return new PlanarPoints(_XLN, new Point3D(_xL, _yL, _zL), new Point3D(_xL, _yL, _zH), new Point3D(_xL, _yH, _zH), new Point3D(_xL, _yH, _zL));
            }

            if (facingPt.X > _xH)
            {
                yield return new PlanarPoints(_XHN, new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yL, _zH), new Point3D(_xH, _yH, _zH), new Point3D(_xH, _yH, _zL));
            }
            // y planes
            if (facingPt.Y < _yL)
            {
                yield return new PlanarPoints(_YLN, new Point3D(_xL, _yL, _zL), new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yL, _zH), new Point3D(_xL, _yL, _zH));
            }

            if (facingPt.Y > _yH)
            {
                yield return new PlanarPoints(_YHN, new Point3D(_xL, _yH, _zL), new Point3D(_xH, _yH, _zL), new Point3D(_xH, _yH, _zH), new Point3D(_xL, _yH, _zH));
            }
            // z planes
            if (facingPt.Z < _zL)
            {
                yield return new PlanarPoints(_ZLN, new Point3D(_xL, _yL, _zL), new Point3D(_xH, _yL, _zL), new Point3D(_xH, _yH, _zL), new Point3D(_xL, _yH, _zL));
            }

            if (facingPt.Z > _zH)
            {
                yield return new PlanarPoints(_ZHN, new Point3D(_xL, _yL, _zH), new Point3D(_xH, _yL, _zH), new Point3D(_xH, _yH, _zH), new Point3D(_xL, _yH, _zH));
            }

            yield break;
        }
        #endregion

        #region protected Vector3D? LowerVector(SliverSlopeParams param, int z, int y, int x, Point3D p1, Point3D p2)
        protected Vector3D? LowerVector(SliverSlopeParams param, int z, int y, int x, Point3D p1, Point3D p2)
        {
            // all intersecting points
            var _points = (from _plane in LowerPlanes(param, z, y, x, p1)
                           let _pt = _plane.SegmentIntersection(p1, p2)
                           where _pt.HasValue
                           select _pt.Value).Distinct().ToList();
            if (_points.Count > 1)
            {
                // get longest vector
                return (from _p in _points
                        from _r in _points
                        select _p - _r).OrderByDescending(_v => _v.LengthSquared).First();
            }

            // none
            return null;
        }
        #endregion

        /// <summary>Indicates that this sliver only blocks transit in the orthogonal direction</summary>
        public bool IsShell { get { return _Shell; } set { _Shell = value; } }

        public override bool IsShadeable(uint param) => true;

        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            var _param = new SliverSlopeParams(param);
            SliverSpaceFaces.AddInnerStructures(_param, this, CellEdge, addToGroup, z, y, x, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            var _param = new SliverSlopeParams(param);
            SliverSpaceFaces.AddOuterSurface(_param, this, CellEdge, group, z, y, x, face, effect, bump);
        }

        #region parameterized indirection
        /// <summary>PlusMaterial occupies the Hi-Range of the Ortho Axis unless FlipFlag is set</summary>
        protected CellMaterial UpperMaterial(SliverSlopeParams param)
            => param.Flip ? CellMaterial : PlusMaterial;

        /// <summary>
        /// CellMaterial occupies the Lo-Range of the Ortho Axis unless FlipFlag is set
        /// </summary>
        protected CellMaterial LowerMaterial(SliverSlopeParams param)
            => param.Flip ? PlusMaterial : CellMaterial;
        #endregion

        #region public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            // decompose to individual parameters
            var _param = new SliverSlopeParams(paramsIn);

            // see if any need to flip
            if (_param.Axis == flipAxis)
            {
                _param.Flip = !_param.Flip;
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
                var _param = new SliverSlopeParams(paramsIn);
                if (axis1 == _param.Axis)
                {
                    _param.Axis = axis2;
                }
                else if (axis2 == _param.Axis)
                {
                    _param.Axis = axis1;
                }
                return _param.Value;
            }
            return paramsIn;
        }
        #endregion

        #region public override bool BlocksDetect(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        public override bool BlocksDetect(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            // upper vector and material
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            if (_upper.DetectBlockingThickness > 0)
            {
                var _upVector = UpperVector(_param, z, y, x, pt1, pt2);
                if (_upVector.HasValue)
                {
                    if (_upVector.Value.Length >= _upper.DetectBlockingThickness)
                    {
                        return true;
                    }
                }
            }

            // lower vector and material
            var _lower = LowerMaterial(_param);
            if (_lower.DetectBlockingThickness > 0)
            {
                var _lowVector = LowerVector(_param, z, y, x, pt1, pt2);
                if (_lowVector.HasValue)
                {
                    if (_lowVector.Value.Length >= _lower.DetectBlockingThickness)
                    {
                        return true;
                    }
                }
            }

            // nothing blocking the detection
            return false;
        }
        #endregion

        #region bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            // upper vector and material
            var _param = new SliverSlopeParams(param);
            var _axis = _param.Axis;
            double _max;
            double _min;
            bool _inMaterial(Point3D pt)
                => _axis switch
                {
                    Axis.X => pt.X >= _min && pt.X <= _max,
                    Axis.Y => pt.Y >= _min && pt.Y <= _max,
                    _ => pt.Z >= _min && pt.Z <= _max,
                };

            if (UpperMaterial(_param).BlocksEffect)
            {
                switch (_axis)
                {
                    case Axis.X:
                        _min = (x * 5.0d) + _param.LoFlippableOffset;
                        _max = (x + 1) * 5.0d;
                        break;

                    case Axis.Y:
                        _min = (y * 5.0d) + _param.LoFlippableOffset;
                        _max = (y + 1) * 5.0d;
                        break;

                    case Axis.Z:
                    default:
                        _min = (z * 5.0d) + _param.LoFlippableOffset;
                        _max = (z + 1) * 5.0d;
                        break;
                }
                if (_inMaterial(pt1) || _inMaterial(pt2))
                {
                    return (_min != _max);
                }
            }

            // lower vector and material
            if (LowerMaterial(_param).BlocksEffect)
            {
                switch (_axis)
                {
                    case Axis.X:
                        _min = x * 5.0d;
                        _max = _min + _param.LoFlippableOffset;
                        break;

                    case Axis.Y:
                        _min = y * 5.0d;
                        _max = _min + _param.LoFlippableOffset;
                        break;

                    case Axis.Z:
                    default:
                        _min = z * 5.0d;
                        _max = _min + _param.LoFlippableOffset;
                        break;
                }
                if (_inMaterial(pt1) || _inMaterial(pt2))
                {
                    return (_min != _max);
                }
            }

            // nothing blocking the effect
            return false;
        }
        #endregion

        // TODO: affect on specific interactions...

        #region public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            // get paramameters
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _fOffset = _param.Offset;
            var _ortho = _param.Axis;

            if (surfaceFace.IsOrthogonalTo(_ortho))
            {
                // trying to cross in the same axis as the sliver
                if (surfaceFace.IsLowFace())
                {
                    return new Tactical.HedralGrip(!movement.CanMoveThrough(_lower));
                }
                else
                {
                    return new Tactical.HedralGrip(!movement.CanMoveThrough(_upper));
                }
            }
            else if (IsShell)
            {
                // ignore calculated blocking
                return new Tactical.HedralGrip(false);
            }
            else
            {
                if (movement.CanMoveThrough(_lower))
                {
                    // can move through cell material
                    if (movement.CanMoveThrough(_upper))
                    {
                        // ... and plus material
                        return new Tactical.HedralGrip(false);
                    }

                    // could only move through cell material (plus blocks)
                    return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetHighFace(), _fOffset);
                }
                else if (movement.CanMoveThrough(_upper))
                {
                    // could only move through plus material (cell blocks)
                    return new HedralGrip(surfaceFace.GetAxis(), _ortho.GetLowFace(), _fOffset);
                }

                // could not move through either material
                return new Tactical.HedralGrip(true);
            }
        }
        #endregion

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
                    MovementOpening _opening(AnchorFace _face, double _off)
                        => new MovementOpening(_face, _off, 1);

                    // params
                    var _fOffset = _param.LoFlippableOffset;
                    var _ortho = _param.Axis;
                    switch (_ortho)
                    {
                        case Axis.Z:
                            if (_lowBlocked)
                            {
                                yield return _opening(AnchorFace.ZHigh, 5d - _fOffset);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.ZLow, _fOffset);
                            }

                            break;

                        case Axis.Y:
                            if (_lowBlocked)
                            {
                                yield return _opening(AnchorFace.YHigh, 5d - _fOffset);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.YLow, _fOffset);
                            }

                            break;

                        default:
                            if (_lowBlocked)
                            {
                                yield return _opening(AnchorFace.XHigh, 5d - _fOffset);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.XLow, _fOffset);
                            }

                            break;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _fOffset = _param.LoFlippableOffset;
            var _ortho = _param.Axis;

            // cannot move through both
            if (!ValidSpace(param, movement))
            {
                var _lowBlocked = !movement.CanMoveThrough(_lower);
                var _highBlocked = !movement.CanMoveThrough(_upper);

                // only have inner elevation on the sliver orthogonal faces
                if (upDirection.GetAxis() == _ortho)
                {
                    if (upDirection.IsLowFace())
                    {
                        if (_highBlocked)
                        {
                            yield return new SlopeSegment
                            {
                                Low = 5d - _fOffset + baseElev,
                                High = 5d - _fOffset + baseElev,
                                Run = 5
                            };
                        }
                    }
                    else if (_lowBlocked)
                    {
                        yield return new SlopeSegment
                        {
                            Low = _fOffset + baseElev,
                            High = _fOffset + baseElev,
                            Run = 5
                        };
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
                // param
                var _param = new SliverSlopeParams(param);
                var _upper = UpperMaterial(_param);
                var _lower = LowerMaterial(_param);
                var _fOffset = _param.LoFlippableOffset;
                var _ortho = _param.Axis;
                var _lowBlocked = !movement.CanMoveThrough(_lower);
                var _highBlocked = !movement.CanMoveThrough(_upper);

                var _zf = _ortho == Axis.Z ? 1d : 0d;
                var _yf = _ortho == Axis.Y ? 1d : 0d;
                var _xf = _ortho == Axis.X ? 1d : 0d;
                var _off = _highBlocked ? _fOffset - 5 : _fOffset;
                return new Vector3D(_xf * _off, _yf * _off, _zf * _off);
            }
            return base.OrthoOffset(param, movement, gravity);
        }
        #endregion

        public override bool ValidSpace(uint param, MovementBase movement)
        {
            // valid for both materials
            return base.ValidSpace(param, movement) && movement.CanMoveThrough(PlusMaterial);
        }

        #region protected bool BlocksMove(SliverSlopeParams param, MovementBase movement, IEnumerable<AnchorFace> faces, bool plus)
        /// <summary>True if this cellspace hinders extension across the specified faces</summary>
        protected bool BlocksMove(SliverSlopeParams param, MovementBase movement, IEnumerable<AnchorFace> faces, bool plus)
        {
            var _ortho = param.Axis;

            // !plus means standard material blocked
            if (!plus && faces.Any(_f => _f.IsOrthogonalTo(_ortho) && _f.IsLowFace()))
            {
                return true;
            }
            if (plus && faces.Any(_f => _f.IsOrthogonalTo(_ortho) && !_f.IsLowFace()))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _cellBlock = !movement.CanMoveThrough(_lower);
            var _plusBlock = !movement.CanMoveThrough(_upper);
            if (_cellBlock && _plusBlock)
            {
                // both materials block
                return true;
            }
            else if (!_cellBlock && !_plusBlock)
            {
                // neither material blocks
                return false;
            }

            return BlocksMove(_param, movement, snap.ToFaceList(), _plusBlock);
            // || edge blocking!
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
            var _displacement = (_param.LoFlippableOffset / 2) - (_loBlock ? 0d : 2.5d);
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
                var _fOffset = _param.LoFlippableOffset;
                var _ortho = _param.Axis;
                var _loZ = ((_ortho == Axis.Z) && _loBlock) ? _fOffset : 0;
                var _hiZ = ((_ortho == Axis.Z) && _hiBlock) ? _fOffset : 5;
                var _loY = ((_ortho == Axis.Y) && _loBlock) ? _fOffset : 0;
                var _hiY = ((_ortho == Axis.Y) && _hiBlock) ? _fOffset : 5;
                var _loX = ((_ortho == Axis.X) && _loBlock) ? _fOffset : 0;
                var _hiX = ((_ortho == Axis.X) && _hiBlock) ? _fOffset : 5;

                // corners (x8)
                yield return new Point3D(_loX, _loY, _loZ);
                yield return new Point3D(_hiX, _loY, _loZ);
                yield return new Point3D(_loX, _hiY, _loZ);
                yield return new Point3D(_hiX, _hiY, _loZ);

                yield return new Point3D(_loX, _loY, _hiZ);
                yield return new Point3D(_hiX, _loY, _hiZ);
                yield return new Point3D(_loX, _hiY, _hiZ);
                yield return new Point3D(_hiX, _hiY, _hiZ);

                // edges (<=12)
                var _midX = (_loX + _hiX) / 2;
                var _midY = (_loY + _hiY) / 2;
                var _midZ = (_loZ + _hiZ) / 2;
                var _zOff = _hiZ - _loZ;
                if (_zOff >= 2.5)
                {
                    yield return new Point3D(_loX, _loY, _midZ);
                    yield return new Point3D(_hiX, _loY, _midZ);
                    yield return new Point3D(_loX, _hiY, _midZ);
                    yield return new Point3D(_hiX, _hiY, _midZ);
                }
                var _yOff = _hiY - _loY;
                if (_yOff >= 2.5)
                {
                    yield return new Point3D(_loX, _midY, _loZ);
                    yield return new Point3D(_hiX, _midY, _loZ);
                    yield return new Point3D(_loX, _midY, _hiZ);
                    yield return new Point3D(_hiX, _midY, _hiZ);
                }
                var _xOff = _hiX - _loX;
                if (_xOff >= 2.5)
                {
                    yield return new Point3D(_midX, _loY, _loZ);
                    yield return new Point3D(_midX, _hiY, _loZ);
                    yield return new Point3D(_midX, _loY, _hiZ);
                    yield return new Point3D(_midX, _hiY, _hiZ);
                }

                // faces (<=5)
                if ((_xOff >= 2.5) && (_yOff >= 2.5))
                {
                    if (_loZ == 0)
                    {
                        yield return new Point3D(_midX, _midY, _loZ);
                    }

                    if (_hiZ == 5)
                    {
                        yield return new Point3D(_midX, _midY, _hiZ);
                    }
                }
                if ((_xOff >= 2.5) && (_zOff >= 2.5))
                {
                    if (_loY == 0)
                    {
                        yield return new Point3D(_midX, _loY, _midZ);
                    }

                    if (_hiY == 5)
                    {
                        yield return new Point3D(_midX, _hiY, _midZ);
                    }
                }
                if ((_yOff >= 2.5) && (_zOff >= 2.5))
                {
                    if (_loX == 0)
                    {
                        yield return new Point3D(_loX, _midY, _midZ);
                    }

                    if (_hiX == 5)
                    {
                        yield return new Point3D(_loY, _midY, _midZ);
                    }
                }

                // center
                if ((_xOff >= 2.5) && (_yOff >= 2.5) && (_zOff >= 2.5))
                {
                    yield return new Point3D(_midX, _midY, _midZ);
                }
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
                var _ortho = _param.Axis;
                var _fOffset = _param.LoFlippableOffset;
                var _loZ = ((_ortho == Axis.Z) && _loBlock) ? _fOffset : 0;
                var _hiZ = ((_ortho == Axis.Z) && _hiBlock) ? _fOffset : 5;
                var _loY = ((_ortho == Axis.Y) && _loBlock) ? _fOffset : 0;
                var _hiY = ((_ortho == Axis.Y) && _hiBlock) ? _fOffset : 5;
                var _loX = ((_ortho == Axis.X) && _loBlock) ? _fOffset : 0;
                var _hiX = ((_ortho == Axis.X) && _hiBlock) ? _fOffset : 5;

                // corners (x8)
                yield return new TargetCorner(new Point3D(_loX, _loY, _loZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(_hiX, _loY, _loZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(_loX, _hiY, _loZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(_hiX, _hiY, _loZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                yield return new TargetCorner(new Point3D(_loX, _loY, _hiZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(_hiX, _loY, _hiZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(_loX, _hiY, _hiZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(_hiX, _hiY, _hiZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
            }
            yield break;
        }
        #endregion

        #region ISliverSpace Members

        public bool IsPlusGas => PlusMaterial is GasCellMaterial;
        public bool IsPlusLiquid => PlusMaterial is LiquidCellMaterial;
        public bool IsPlusInvisible => IsPlusGas && (PlusMaterial as GasCellMaterial).IsInvisible;
        public bool IsPlusSolid => PlusMaterial is SolidCellMaterial;

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => SliverSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => SliverSpaceFaces.ShowFace(param, this, outwardFace);

        #region public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (PlusTiling == null)
            {
                return new BuildableMaterial { Material = null, IsAlpha = false };
            }

            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.ZPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.ZMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.YPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.YMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.XPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.XMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }
        #endregion

        #region public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
        {
            return new BuildableMaterial { Material = null, IsAlpha = false };
        }
        #endregion

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = PlusTiling?.BrushCollectionKey ?? string.Empty,
                BrushIndex = PlusTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };
        }

        #endregion

        public string PlusMaterialName
            => PlusMaterial?.Name ?? string.Empty;

        public string PlusTilingName
            => PlusTiling?.Name ?? string.Empty;

        public override CellSpaceInfo ToCellSpaceInfo()
            => new SliverSpaceInfo(this, CellEdge);

        #region public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            // param
            var _param = new SliverSlopeParams(param);
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlocked = !movement.CanMoveThrough(_lower);
            var _hiBlocked = !movement.CanMoveThrough(_upper);
            var _plusBlocked = !movement.CanMoveThrough(PlusMaterial);

            // only 1 material must block for inner climb
            if (_loBlocked ^ _hiBlocked)
            {
                var _axis = _param.Axis;
                var _faces = _loBlocked
                    ? _axis.GetLowFace().ToAnchorFaceList()
                    : _axis.GetHighFace().ToAnchorFaceList();
                if (_axis != gravity.GetAxis())
                {
                    // climbing across the inner face
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerBase(_plusBlocked),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }

                // dangling?
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

                // perfect grip 
                return new CellGripResult
                {
                    Difficulty = -5,
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

        protected virtual GripDisposition GetGripDisposition(SliverSlopeParams param, AnchorFace gripFace, Axis axis)
            => axis == gripFace.GetAxis()
            ? GripDisposition.Full
            : GripDisposition.Rectangular;

        #region public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            // plus material HI or LO?
            var _param = new SliverSlopeParams(param);
            var _flip = _param.Flip;

            // plus gripping?
            var _isPlusGrip = !(gripFace.IsLowFace() ^ _flip);
            var _axis = _param.Axis;

            // param expansion to cell characteristics
            var _upper = UpperMaterial(_param);
            var _lower = LowerMaterial(_param);
            var _loBlocked = !movement.CanMoveThrough(_lower);
            var _hiBlocked = !movement.CanMoveThrough(_upper);
            var _sliverFace = _loBlocked ? _axis.GetLowFace() : _axis.GetHighFace();

            // get blocking difference
            var _grip = HedralGripping(param, movement, gripFace);
            var _mask = sourceStruc.HedralGripping(movement, gripFace.ReverseFace()).Invert();
            var _usable = _grip.Intersect(_mask);
            var _count = _usable.GripCount();
            if (_count <= 3)
            {
                return null;
            }

            // disposition
            var _disposition = GetGripDisposition(_param, gripFace, _axis);

            if (gravity == gripFace)
            {
                // dangling...
                if (_disposition == GripDisposition.Full)
                {
                    return GripRules.GetOuterDangling(_isPlusGrip);
                }

                return GripRules.GetOuterDangling(_disposition);
            }

            // full face grip
            if (_disposition == GripDisposition.Full)
            {
                return GripRules.GetOuterBase(_isPlusGrip);
            }

            var _gravLow = gravity.IsLowFace();

            // ledge when (gravity and ortho axis are the same) AND (only one material blocks) ...
            // ... AND ((grav is low with a low blocker) OR (grav is high with a high blocker))
            if ((gravity.GetAxis() == _axis) && (_loBlocked ^ _hiBlocked)
                && ((_gravLow && _loBlocked) || (!_gravLow && _hiBlocked)))
            {
                // using plus material if ONLY gravity is not low (ie, high) XOR cell is flipped
                return GripRules.GetOuterLedge((!_gravLow) ^ _flip);
            }

            // mixed face grip without ledge
            return GripRules.GetOuterBase(_disposition);
        }
        #endregion

        public override int? InnerSwimDifficulty(uint param)
            => (new[] { base.InnerSwimDifficulty(param), MaterialSwimDifficulty(PlusMaterial) }).Max();

        public override bool SuppliesBreathableAir(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<GasCellMaterial>()
            .Any(_gas => _gas.AirBreathe);

        public override bool SuppliesBreathableWater(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<LiquidCellMaterial>()
            .Any(_liquid => _liquid.AquaticBreathe);
    }
}