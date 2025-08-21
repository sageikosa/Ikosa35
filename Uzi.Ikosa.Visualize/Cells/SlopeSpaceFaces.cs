using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows;

namespace Uzi.Visualize
{
    public static class SlopeSpaceFaces
    {
        #region data
        private static MatrixTransform3D _ZX;
        private static MatrixTransform3D _ZY;
        private static MatrixTransform3D _YX;
        private static MatrixTransform3D _YZ;
        private static MatrixTransform3D _XZ;
        private static MatrixTransform3D _XY;

        private static Vector3D _XLN;
        private static Vector3D _XHN;
        private static Vector3D _YLN;
        private static Vector3D _YHN;
        private static Vector3D _ZLN;
        private static Vector3D _ZHN;

        private static CornerPoints _CornerPoints;
        #endregion

        #region static setup
        static SlopeSpaceFaces()
        {
            var _center = new Point3D(2.5, 2.5, 2.5);
            _ZX = new MatrixTransform3D((new TranslateTransform3D()).Value);
            _ZY = new MatrixTransform3D((new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90), _center)).Value);

            var _y = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, 0, 0), 90), _center);
            _YX = new MatrixTransform3D(_y.Value);
            var _yzGrp = new Transform3DGroup();
            _yzGrp.Children.Add(_y);
            _yzGrp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, -1, 0), 90), _center));
            _YZ = new MatrixTransform3D(_yzGrp.Value);

            var _x = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90), _center);
            var _xyGrp = new Transform3DGroup();
            _xyGrp.Children.Add(_x);
            _xyGrp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), _center));
            _XY = new MatrixTransform3D(_xyGrp.Value);
            var _xzGrp = new Transform3DGroup();
            _xzGrp.Children.Add(_x);
            _xzGrp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180), _center));
            _XZ = new MatrixTransform3D(_xzGrp.Value);

            // freeze all
            _ZX.Freeze();
            _ZY.Freeze();
            _YX.Freeze();
            _YZ.Freeze();
            _XY.Freeze();
            _XZ.Freeze();

            _XLN = AnchorFace.XLow.GetNormalVector();
            _XHN = AnchorFace.XHigh.GetNormalVector();
            _YLN = AnchorFace.YLow.GetNormalVector();
            _YHN = AnchorFace.YHigh.GetNormalVector();
            _ZLN = AnchorFace.ZLow.GetNormalVector();
            _ZHN = AnchorFace.ZHigh.GetNormalVector();

            _CornerPoints = new CornerPoints
            {
                Point0 = new Point3D(0, 0, 0),
                Point1 = new Point3D(0, 0, 5),
                Point2 = new Point3D(0, 5, 0),
                Point3 = new Point3D(0, 5, 5),
                Point4 = new Point3D(5, 0, 0),
                Point5 = new Point3D(5, 0, 5),
                Point6 = new Point3D(5, 5, 0),
                Point7 = new Point3D(5, 5, 5)
            };
        }
        #endregion

        #region public static Transform3D GetOrthoAndSlopeTransform(Axis ortho, Axis slope)
        /// <summary>Gets transform needed to rotate Z-ortho, X-sloped Slope into target ortho and slope</summary>
        public static Transform3D GetOrthoAndSlopeTransform(Axis ortho, Axis slope)
        {
            switch (ortho)
            {
                case Axis.Z:
                    switch (slope)
                    {
                        case Axis.Y:
                            return _ZY;
                        case Axis.X:
                        default:
                            return _ZX;
                    }

                case Axis.Y:
                    switch (slope)
                    {
                        case Axis.X:
                            return _YX;
                        case Axis.Z:
                        default:
                            return _YZ;
                    }

                case Axis.X:
                default:
                    switch (slope)
                    {
                        case Axis.Y:
                            return _XY;

                        case Axis.Z:
                        default:
                            return _XZ;
                    }
            }
        }
        #endregion

        #region private static IEnumerable<PlanarPoints> GetPlanes(CornerPoints corners)
        private static IEnumerable<PlanarPoints> GetPlanes(CornerPoints corners, AnchorFace sloped)
        {
            yield return GetPlane(corners, AnchorFace.ZHigh, sloped);
            yield return GetPlane(corners, AnchorFace.YHigh, sloped);
            yield return GetPlane(corners, AnchorFace.XHigh, sloped);
            yield return GetPlane(corners, AnchorFace.ZLow, sloped);
            yield return GetPlane(corners, AnchorFace.YLow, sloped);
            yield return GetPlane(corners, AnchorFace.XLow, sloped);
            yield break;
        }
        #endregion

        #region public static IEnumerable<PlanarPoints> GeneratePlanes(SliverSlopeParams param, int z, int y, int x, bool isUpper)
        public static IEnumerable<PlanarPoints> GeneratePlanes(SliverSlopeParams param, int z, int y, int x, bool isUpper, Point3D facingPt)
        {
            // params
            var _ortho = param.Axis;
            var _slope = param.SlopeAxis;
            var _loOffset = param.LoFlippableOffset;
            var _hiOffset = param.HiFlippableOffset;
            var _sloped = isUpper ? _ortho.GetLowFace() : _ortho.GetHighFace();

            // return planes
            var _corners = GetSlopeCorners(z, y, x, isUpper, _ortho, _slope, _loOffset, _hiOffset);
            if (facingPt.Z > ((z + 1) * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.ZHigh, _sloped);
            }

            if (facingPt.Y > ((y + 1) * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.YHigh, _sloped);
            }

            if (facingPt.X > ((x + 1) * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.XHigh, _sloped);
            }

            if (facingPt.Z < (z * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.ZLow, _sloped);
            }

            if (facingPt.Y < (y * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.YLow, _sloped);
            }

            if (facingPt.X < (x * 5d))
            {
                yield return GetPlane(_corners, AnchorFace.XLow, _sloped);
            }

            yield break;
        }
        #endregion

        public static IEnumerable<PlanarPoints> GeneratePlanes(int z, int y, int x, bool isUpper, Axis ortho, Axis slope, double loOffset, double hiOffset)
        {
            // return planes
            return GetPlanes(GetSlopeCorners(z, y, x, isUpper, ortho, slope, loOffset, hiOffset), isUpper ? ortho.GetLowFace() : ortho.GetHighFace());
        }

        #region private static PlanarPoints GetPlane(CornerPoints corners, AnchorFace face)
        private static PlanarPoints GetPlane(CornerPoints corners, AnchorFace face, AnchorFace sloped)
        {
            // NOTE: these are setup so the texture coordinates "match"
            if (face == sloped)
            {
                switch (face)
                {
                    case AnchorFace.ZHigh: return new PlanarPoints(corners.Point1, corners.Point5, corners.Point7, corners.Point3); // Z Hi
                    case AnchorFace.YHigh: return new PlanarPoints(corners.Point6, corners.Point2, corners.Point3, corners.Point7); // Y Hi
                    case AnchorFace.XHigh: return new PlanarPoints(corners.Point4, corners.Point6, corners.Point7, corners.Point5); // X Hi
                    case AnchorFace.ZLow: return new PlanarPoints(corners.Point4, corners.Point0, corners.Point2, corners.Point6); // Z Lo
                    case AnchorFace.YLow: return new PlanarPoints(corners.Point0, corners.Point4, corners.Point5, corners.Point1); // Y Lo
                    case AnchorFace.XLow: return new PlanarPoints(corners.Point2, corners.Point0, corners.Point1, corners.Point3); // X Lo
                    default: return new PlanarPoints(corners.Point2, corners.Point0, corners.Point1, corners.Point3); // X Lo
                }
            }
            else
            {
                switch (face)
                {
                    case AnchorFace.ZHigh: return new PlanarPoints(_ZHN, corners.Point1, corners.Point5, corners.Point7, corners.Point3); // Z Hi
                    case AnchorFace.YHigh: return new PlanarPoints(_YHN, corners.Point6, corners.Point2, corners.Point3, corners.Point7); // Y Hi
                    case AnchorFace.XHigh: return new PlanarPoints(_XHN, corners.Point4, corners.Point6, corners.Point7, corners.Point5); // X Hi
                    case AnchorFace.ZLow: return new PlanarPoints(_ZLN, corners.Point4, corners.Point0, corners.Point2, corners.Point6); // Z Lo
                    case AnchorFace.YLow: return new PlanarPoints(_YLN, corners.Point0, corners.Point4, corners.Point5, corners.Point1); // Y Lo
                    case AnchorFace.XLow: return new PlanarPoints(_XLN, corners.Point2, corners.Point0, corners.Point1, corners.Point3); // X Lo
                    default: return new PlanarPoints(_XLN, corners.Point2, corners.Point0, corners.Point1, corners.Point3); // X Lo
                }
            }
        }
        #endregion

        #region private static Point3D[] GetSlopeCorners(int z, int y, int x, bool adjustLo, Axis ortho, Axis slope, double loOffset, double hiOffset)
        private static CornerPoints GetSlopeCorners(int z, int y, int x, bool adjustLo, Axis ortho, Axis slope, double loOffset, double hiOffset)
        {
            // if isUpper, then lower plane is sloped (hence = 0)
            var _upDn = adjustLo ? (byte)0 : (byte)1;
            Func<int, int, int, int> _idx = (hx, hy, hz) => (hx * 4) + (hy * 2) + hz;
            var _pts = _CornerPoints;

            // adjust points
            switch (ortho)
            {
                case Axis.Z:
                    switch (slope)
                    {
                        case Axis.Y:
                            _pts.SetZPoint(_idx(0, 0, _upDn), loOffset);
                            _pts.SetZPoint(_idx(0, 1, _upDn), hiOffset);
                            _pts.SetZPoint(_idx(1, 0, _upDn), loOffset);
                            _pts.SetZPoint(_idx(1, 1, _upDn), hiOffset);
                            break;

                        case Axis.X:
                        default:
                            _pts.SetZPoint(_idx(0, 0, _upDn), loOffset);
                            _pts.SetZPoint(_idx(1, 0, _upDn), hiOffset);
                            _pts.SetZPoint(_idx(0, 1, _upDn), loOffset);
                            _pts.SetZPoint(_idx(1, 1, _upDn), hiOffset);
                            break;
                    }
                    break;

                case Axis.Y:
                    switch (slope)
                    {
                        case Axis.Z:
                            _pts.SetYPoint(_idx(0, _upDn, 0), loOffset);
                            _pts.SetYPoint(_idx(0, _upDn, 1), hiOffset);
                            _pts.SetYPoint(_idx(1, _upDn, 0), loOffset);
                            _pts.SetYPoint(_idx(1, _upDn, 1), hiOffset);
                            break;

                        case Axis.X:
                        default:
                            _pts.SetYPoint(_idx(0, _upDn, 0), loOffset);
                            _pts.SetYPoint(_idx(1, _upDn, 0), hiOffset);
                            _pts.SetYPoint(_idx(0, _upDn, 1), loOffset);
                            _pts.SetYPoint(_idx(1, _upDn, 1), hiOffset);
                            break;
                    }
                    break;

                case Axis.X:
                default:
                    switch (slope)
                    {
                        case Axis.Y:
                            _pts.SetXPoint(_idx(_upDn, 0, 0), loOffset);
                            _pts.SetXPoint(_idx(_upDn, 1, 0), hiOffset);
                            _pts.SetXPoint(_idx(_upDn, 0, 1), loOffset);
                            _pts.SetXPoint(_idx(_upDn, 1, 1), hiOffset);
                            break;

                        case Axis.Z:
                        default:
                            _pts.SetXPoint(_idx(_upDn, 0, 0), loOffset);
                            _pts.SetXPoint(_idx(_upDn, 0, 1), hiOffset);
                            _pts.SetXPoint(_idx(_upDn, 1, 0), loOffset);
                            _pts.SetXPoint(_idx(_upDn, 1, 1), hiOffset);
                            break;
                    }
                    break;
            }

            // move to position
            _pts.AddVector(x * 5.0d, y * 5.0d, z * 5.0d);

            // done
            return _pts;
        }
        #endregion

        #region private static void GenerateInnerSubModel(SliverSlopeParams param, int z, int y, int x, IPlusCellSpace plus, BuildableGroup group, bool isPlus, VisualEffect effect)
        private static void GenerateInnerSubModel(SliverSlopeParams param, int z, int y, int x, IPlusCellSpace plus, BuildableGroup group, bool isPlus, VisualEffect effect)
        {
            // params
            var _ortho = param.Axis;
            var _slope = param.SlopeAxis;
            var _loOffset = param.LoFlippableOffset;
            var _hiOffset = param.HiFlippableOffset;
            var _sFace = isPlus ? _ortho.GetLowFace() : _ortho.GetHighFace();

            // build mesh
            var _pts = GetPlane(GetSlopeCorners(z, y, x, isPlus, _ortho, _slope, _loOffset, _hiOffset), _sFace, _sFace);
            group.Context.GetBuildableMesh(
                isPlus ? plus.GetPlusBuildableMeshKey(_ortho.GetLowFace(), effect) : plus.GetBuildableMeshKey(_ortho.GetHighFace(), effect),
                () => isPlus ? plus.GetPlusOrthoFaceMaterial(_ortho, false, effect) : plus.GetOrthoFaceMaterial(_ortho, true, effect))
                .AddQuad(_pts[0], _pts[1], _pts[2], _pts[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));
        }
        #endregion

        #region public static void AddInnerStructures(SliverSlopeParams param, IPlusCellSpace plus, Model3DGroup addToGroup, int z, int y, int x, VisualEffect effect)
        public static void AddInnerStructures(SliverSlopeParams param, IPlusCellSpace plus, ICellEdge edge, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            var _slope = plus;
            var _flipAxis = param.Flip;
            if (_flipAxis)
            {
                _slope = new SlopeInverter(plus);
            }

            if (!(_slope.IsGas && _slope.IsInvisible))
            {
                GenerateInnerSubModel(param, z, y, x, _slope, addToGroup, false, effect);
            }

            if (!(_slope.IsPlusGas && _slope.IsPlusInvisible))
            {
                GenerateInnerSubModel(param, z, y, x, _slope, addToGroup, true, effect);
            }

            foreach (var _edge in param.GetSlopeEdges(edge))
            {
                WedgeSpaceFaces.BuildInnerStructures(addToGroup, z, y, x, _edge.Axis, _edge.PrimeOff, _edge.SecondOff,
                    true, true, false,
                    (face) => edge.GetBuildableMeshKey(face, effect),
                    (face) => edge.GetBuildableMeshKey(face, effect),
                    (face) => (Func<BuildableMaterial>)(() => _edge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect)),
                    (face) => (Func<BuildableMaterial>)(() => _edge.GetPlusOrthoFaceMaterial(face.GetAxis(), face.IsLowFace(), effect)),
                    () => _edge.GetOtherFaceMaterial(0, effect),
                    () => _edge.GetPlusOtherFaceMaterial(0, effect));
            }
        }
        #endregion

        #region public static void AddInnerSlopeComposite(BuildableGroup group, PanelParams param, double thickness, double slopeThickness, VisualEffect effect, IBasePanel panel)
        public static void AddInnerSlopeComposite(BuildableGroup group, PanelParams param, double thickness, double slopeThickness,
            VisualEffect effect, IBasePanel panel)
        {
            if (param.IsTrueSlope)
            {
                // source
                var _source = param.SourceFace;
                var _ortho = _source.GetAxis();
                var _adjustLo = !_source.IsLowFace();

                // sink
                var _sink = param.SinkFace;
                var _slope = _sink.GetAxis();

                // offsets
                var _lesser = Math.Min(thickness, slopeThickness);
                var _greater = Math.Max(thickness, slopeThickness);
                var _loOffset = _sink.IsLowFace()
                    ? (_adjustLo ? 5 - _greater : _greater)
                    : (_adjustLo ? 5 - _lesser : _lesser);
                var _hiOffset = _sink.IsLowFace()
                    ? (_adjustLo ? 5 - _lesser : _lesser)
                    : (_adjustLo ? 5 - _greater : _greater);
                var _sFace = _adjustLo ? _ortho.GetLowFace() : _ortho.GetHighFace();

                // build mesh
                var _builder = new MeshBuilder();
                var _pts = GetPlane(GetSlopeCorners(0, 0, 0, _adjustLo, _ortho, _slope, _loOffset, _hiOffset),
                    _sFace, _sFace);
                _builder.AddQuad(_pts[0], _pts[1], _pts[2], _pts[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));
                var _mesh = _builder.ToMesh(true);

                // build face
                var _material = panel.GetSideFaceMaterial(SideIndex.Front, effect);
                var _face = new GeometryModel3D(_mesh, _material.Material);
                CellSpaceFaces.AddGeometry(_material.IsAlpha ? group.Alpha : group.Opaque, _face);
            }
        }
        #endregion

        #region private static void GenerateMesh(Axis orthoAxis, Axis slopeAxis, AnchorFace face, bool isPlusMaterial, double loOffset, double hiOffset, BuildableGroup addToGroup, params Vector3D[] trans)
        private static void GenerateMesh(Axis orthoAxis, Axis slopeAxis, AnchorFace face,
            bool isPlusMaterial, double loOffset, double hiOffset,
            BuildableGroup addToGroup, BuildableMeshKey meshKey, Func<BuildableMaterial> material,
            params Vector3D[] trans)
        {
            var _textureSize = new Vector(5d, 5d);
            var _minOff = Math.Min(loOffset, hiOffset);
            var _maxOff = Math.Max(loOffset, hiOffset);
            var _triExt = _maxOff - _minOff;
            var _doTri = _triExt != 0;
            var _faceAxis = face.GetAxis();

            void _addRect(Rect rect)
                => addToGroup.Context.GetBuildableMesh(meshKey, material)
                .AddRectangularMesh(rect, _textureSize, true, face, false, trans);

            void _addTri(Rect rect, TriangleCorner corner)
            {
                if (_doTri)
                {
                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                      .AddRightTriangularMesh(rect, corner, _textureSize, true, face, false, trans);
                }
            }

            // generate model, apply material, and move into place
            switch (orthoAxis)
            {
                case Axis.Y:
                    #region Axis Y
                    if (isPlusMaterial)
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                                if (slopeAxis == Axis.X)
                                {
                                    _addRect(new Rect(0, 0, 5d - loOffset, 5d));
                                }
                                else
                                {
                                    _addRect(new Rect(0, 0, 5d - _maxOff, 5d));
                                    _addTri(new Rect(5d - _maxOff, 0d, _triExt, 5d),
                                        loOffset < hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.UpperLeft);
                                }
                                break;
                            case AnchorFace.XHigh:
                                if (slopeAxis == Axis.X)
                                {
                                    _addRect(new Rect(hiOffset, 0, 5d - hiOffset, 5d));
                                }
                                else
                                {
                                    _addRect(new Rect(_maxOff, 0, 5d - _maxOff, 5d));
                                    _addTri(new Rect(_minOff, 0d, _triExt, 5d),
                                        loOffset > hiOffset ? TriangleCorner.UpperRight : TriangleCorner.LowerRight);
                                }
                                break;

                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;

                            case AnchorFace.ZLow:
                                if (slopeAxis == Axis.Z)
                                {
                                    _addRect(new Rect(0, loOffset, 5d, 5d - loOffset));
                                }
                                else
                                {
                                    _addRect(new Rect(0, _maxOff, 5d, 5d - _maxOff));
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft);
                                }
                                break;
                            default: // ZHigh
                                if (slopeAxis == Axis.Z)
                                {
                                    _addRect(new Rect(0, hiOffset, 5d, 5d - hiOffset));
                                }
                                else
                                {
                                    _addRect(new Rect(0, _maxOff, 5d, 5d - _maxOff));
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight);
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                                if (slopeAxis == Axis.X)
                                {
                                    _addRect(new Rect(5d - loOffset, 0, loOffset, 5d));
                                }
                                else
                                {
                                    _addRect(new Rect(5d - _minOff, 0, _minOff, 5d));
                                    _addTri(new Rect(5d - _maxOff, 0, _triExt, 5d),
                                        loOffset > hiOffset ? TriangleCorner.LowerRight : TriangleCorner.UpperRight);
                                }
                                break;
                            case AnchorFace.XHigh:
                                if (slopeAxis == Axis.X)
                                {
                                    _addRect(new Rect(0, 0, hiOffset, 5d));
                                }
                                else
                                {
                                    _addRect(new Rect(0, 0, _minOff, 5d));
                                    _addTri(new Rect(_minOff, 0, _triExt, 5d),
                                        loOffset > hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.UpperLeft);
                                }
                                break;

                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;

                            case AnchorFace.ZLow:
                                if (slopeAxis == Axis.Z)
                                {
                                    _addRect(new Rect(0, 0, 5d, loOffset));
                                }
                                else
                                {
                                    _addRect(new Rect(0, 0, 5d, _minOff));
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset > hiOffset ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft);
                                }
                                break;
                            default: // ZHigh
                                if (slopeAxis == Axis.Z)
                                {
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddRectangularMesh(new Rect(0, 0, 5d, hiOffset),
                                        _textureSize, true, face, false, trans);
                                }
                                else
                                {
                                    _addRect(new Rect(0, 0, 5d, _minOff));
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset > hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight);
                                }
                                break;
                        }
                    }
                    break;
                #endregion

                case Axis.Z:
                    #region Axis Z
                    if (isPlusMaterial)
                    {
                        if ((_faceAxis != orthoAxis) && (_faceAxis != slopeAxis))
                        {
                            _addRect(new Rect(0, _maxOff, 5d, 5d - _maxOff));
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.YHigh:
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft);
                                    break;
                                case AnchorFace.XHigh:
                                case AnchorFace.YLow:
                                default:
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight);
                                    break;
                            }
                        }
                        else
                        {
                            #region rect
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.YLow:
                                    _addRect(new Rect(0d, loOffset, 5d, 5d - loOffset));
                                    break;
                                case AnchorFace.XHigh:
                                case AnchorFace.YHigh:
                                    _addRect(new Rect(0d, hiOffset, 5d, 5d - hiOffset));
                                    break;
                                case AnchorFace.ZLow:
                                default:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddCellFace(face, trans);
                                    break;
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        _addRect(new Rect(0d, 0d, 5d, _minOff));
                        if ((_faceAxis != orthoAxis) && (_faceAxis != slopeAxis))
                        {
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.YHigh:
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight);
                                    break;
                                case AnchorFace.XHigh:
                                case AnchorFace.YLow:
                                    _addTri(new Rect(0, _minOff, 5d, _triExt),
                                        loOffset < hiOffset ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft);
                                    break;
                            }
                        }
                        else
                        {
                            #region rect
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.YLow:
                                    _addRect(new Rect(0d, 0d, 5d, loOffset));
                                    break;
                                case AnchorFace.XHigh:
                                case AnchorFace.YHigh:
                                    _addRect(new Rect(0d, 0d, 5d, hiOffset));
                                    break;
                                case AnchorFace.ZLow:
                                default:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddCellFace(face, trans);
                                    break;
                            }
                            #endregion
                        }
                    }
                    break;
                #endregion

                case Axis.X:
                    #region Axis X
                    if (isPlusMaterial)
                    {
                        if ((_faceAxis != orthoAxis) && (_faceAxis != slopeAxis))
                        {
                            switch (face)
                            {
                                case AnchorFace.YLow:
                                case AnchorFace.ZHigh:
                                    _addRect(new Rect(_maxOff, 0, 5d - _maxOff, 5d));
                                    _addTri(new Rect(_minOff, 0, _triExt, 5d),
                                        loOffset < hiOffset ? TriangleCorner.LowerRight : TriangleCorner.UpperRight);
                                    break;
                                case AnchorFace.YHigh:
                                case AnchorFace.ZLow:
                                    _addRect(new Rect(0, 0, 5d - _maxOff, 5d));
                                    _addTri(new Rect(5d - _maxOff, 0, _triExt, 5d),
                                        loOffset < hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.UpperLeft);
                                    break;
                            }
                        }
                        else
                        {
                            #region rect
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.XHigh:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddCellFace(face, trans);
                                    break;
                                case AnchorFace.YHigh:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddRectangularMesh(new Rect(0, 0, 5d - hiOffset, 5d),
                                        _textureSize, true, face, false, trans);
                                    break;
                                case AnchorFace.YLow:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddRectangularMesh(new Rect(loOffset, 0, 5d - loOffset, 5d),
                                        _textureSize, true, face, false, trans);
                                    break;
                                case AnchorFace.ZLow:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddRectangularMesh(new Rect(0, 0, 5d - loOffset, 5d),
                                        _textureSize, true, face, false, trans);
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddRectangularMesh(new Rect(hiOffset, 0, 5d - hiOffset, 5d),
                                        _textureSize, true, face, false, trans);
                                    break;
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        if ((_faceAxis != orthoAxis) && (_faceAxis != slopeAxis))
                        {
                            switch (face)
                            {
                                case AnchorFace.YLow:
                                case AnchorFace.ZHigh:
                                    _addRect(new Rect(0, 0, _minOff, 5d));
                                    _addTri(new Rect(_minOff, 0, _triExt, 5d),
                                        loOffset > hiOffset ? TriangleCorner.LowerLeft : TriangleCorner.UpperLeft);
                                    break;
                                case AnchorFace.YHigh:
                                case AnchorFace.ZLow:
                                    _addRect(new Rect(5d - _minOff, 0, _minOff, 5d));
                                    _addTri(new Rect(5d - _maxOff, 0, _triExt, 5d),
                                        loOffset > hiOffset ? TriangleCorner.LowerRight : TriangleCorner.UpperRight);
                                    break;
                            }
                        }
                        else
                        {
                            #region rect
                            switch (face)
                            {
                                case AnchorFace.XLow:
                                case AnchorFace.XHigh:
                                    addToGroup.Context.GetBuildableMesh(meshKey, material)
                                        .AddCellFace(face, trans);
                                    break;
                                case AnchorFace.YLow:
                                    _addRect(new Rect(0, 0, loOffset, 5d));
                                    break;
                                case AnchorFace.YHigh:
                                    _addRect(new Rect(5d - hiOffset, 0, hiOffset, 5d));
                                    break;
                                case AnchorFace.ZLow:
                                    _addRect(new Rect(5d - loOffset, 0, loOffset, 5d));
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    _addRect(new Rect(0, 0, hiOffset, 5d));
                                    break;
                            }
                            #endregion
                        }
                    }
                    break;
                    #endregion
            }
        }
        #endregion

        #region private static void GenerateOuterSubModel(SliverSlopeParams param, IPlusCellSpace slope, BuildableGroup group, bool showInner, int z, int y, int x, bool isPlus, VisualEffect effect, Transform3D bump)
        private static void GenerateOuterSubModel(SliverSlopeParams param, IPlusCellSpace slope, BuildableGroup group, bool showInner,
            int z, int y, int x, bool isPlus, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            #region face visible logic
            switch (face)
            {
                case AnchorFace.ZLow:
                    z--;
                    break;

                case AnchorFace.ZHigh:
                    z++;
                    break;

                case AnchorFace.YLow:
                    y--;
                    break;

                case AnchorFace.YHigh:
                    y++;
                    break;

                case AnchorFace.XLow:
                    x--;
                    break;

                case AnchorFace.XHigh:
                    x++;
                    break;
            }
            #endregion

            // params
            var _ortho = param.Axis;
            var _slopeAxis = param.SlopeAxis;
            var _loOffset = param.LoFlippableOffset;
            var _hiOffset = param.HiFlippableOffset;

            #region Inward visibility Tests
            // inward faces (liquids and non-invisible gases)
            if (showInner)
            {
                if (!isPlus &&
                    (((_ortho == Axis.X) && (face == AnchorFace.XHigh))
                    || ((_ortho == Axis.Y) && (face == AnchorFace.YHigh))
                    || ((_ortho == Axis.Z) && (face == AnchorFace.ZHigh))))
                {
                    // minus material does not expose itself to the upper face of the ortho axis
                    return;
                }
                else if (isPlus &&
                    (((_ortho == Axis.X) && (face == AnchorFace.XLow))
                    || ((_ortho == Axis.Y) && (face == AnchorFace.YLow))
                    || ((_ortho == Axis.Z) && (face == AnchorFace.ZLow))))
                {
                    // plus material does not expose itself to the lower face of the ortho axis
                    return;
                }
            }
            #endregion

            // TODO: hidden face optimizations
            var _builder = isPlus
                ? (Func<BuildableMaterial>)(() => slope.GetPlusOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect))
                : (Func<BuildableMaterial>)(() => slope.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect));

            var _meshKey = isPlus
                ? slope.GetPlusBuildableMeshKey(face, effect)
                : slope.GetBuildableMeshKey(face, effect);

            // mesh and faces
            var _move = new Vector3D(x * 5, y * 5, z * 5);

            // generate model, apply material, and move into place
            GenerateMesh(_ortho, _slopeAxis, face, isPlus, _loOffset, _hiOffset,
                group, _meshKey, _builder, _move, bump);
        }
        #endregion

        public static void AddOuterSurface(SliverSlopeParams param, IPlusCellSpace plus, ICellEdge edge,
            BuildableGroup addToGroup, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            var _slope = plus;
            var _flipAxis = param.Flip;
            if (_flipAxis)
            {
                _slope = new SlopeInverter(plus);
            }

            if (!_slope.IsInvisible)
            {
                GenerateOuterSubModel(param, _slope, addToGroup, _slope.IsGas || _slope.IsLiquid,
                    z, y, x, false, face, effect, bump);
            }

            if (!_slope.IsPlusInvisible)
            {
                GenerateOuterSubModel(param, _slope, addToGroup, _slope.IsPlusGas || _slope.IsPlusLiquid,
                    z, y, x, true, face, effect, bump); // NOTE: fixed(?) isPlus flag
            }

            Func<BuildableMaterial> _builder =
                () => edge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            var _move = new Vector3D(x * 5, y * 5, z * 5);

            var _meshKey = edge.GetBuildableMeshKey(face, effect);
            foreach (var _edge in param.GetSlopeEdges(edge))
            {
                WedgeSpaceFaces.BuildOuterWedgeModel(addToGroup, face, _edge.Axis, _edge.PrimeOff, _edge.SecondOff, true,
                    _meshKey, _builder, _move);
            }
        }

        private struct CornerPoints
        {
            public Point3D Point0;
            public Point3D Point1;
            public Point3D Point2;
            public Point3D Point3;
            public Point3D Point4;
            public Point3D Point5;
            public Point3D Point6;
            public Point3D Point7;

            public void AddVector(double x, double y, double z)
            {
                Point0.Offset(x, y, z);
                Point1.Offset(x, y, z);
                Point2.Offset(x, y, z);
                Point3.Offset(x, y, z);
                Point4.Offset(x, y, z);
                Point5.Offset(x, y, z);
                Point6.Offset(x, y, z);
                Point7.Offset(x, y, z);
            }

            public void SetZPoint(int index, double z)
            {
                switch (index)
                {
                    case 0: Point0.Z = z; break;
                    case 1: Point1.Z = z; break;
                    case 2: Point2.Z = z; break;
                    case 3: Point3.Z = z; break;
                    case 4: Point4.Z = z; break;
                    case 5: Point5.Z = z; break;
                    case 6: Point6.Z = z; break;
                    default: Point7.Z = z; break;
                }
            }

            public void SetYPoint(int index, double y)
            {
                switch (index)
                {
                    case 0: Point0.Y = y; break;
                    case 1: Point1.Y = y; break;
                    case 2: Point2.Y = y; break;
                    case 3: Point3.Y = y; break;
                    case 4: Point4.Y = y; break;
                    case 5: Point5.Y = y; break;
                    case 6: Point6.Y = y; break;
                    default: Point7.Y = y; break;
                }
            }

            public void SetXPoint(int index, double x)
            {
                switch (index)
                {
                    case 0: Point0.X = x; break;
                    case 1: Point1.X = x; break;
                    case 2: Point2.X = x; break;
                    case 3: Point3.X = x; break;
                    case 4: Point4.X = x; break;
                    case 5: Point5.X = x; break;
                    case 6: Point6.X = x; break;
                    default: Point7.X = x; break;
                }
            }
        }
    }
}
