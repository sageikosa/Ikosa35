using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public static class CylinderSpaceFaces
    {
        const int MINSEG = 2;
        const int MAXSEG = 6;

        #region static ctor()
        static CylinderSpaceFaces()
        {
            // transform for meshes
            var _trans = new Transform3DGroup();
            _trans.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));
            _trans.Children.Add(new TranslateTransform3D(new Vector3D(2.5d, 0, -2.5d)));

            _Smooth = new Dictionary<int, MeshGeometry3D>();
            _Facet = new Dictionary<int, MeshGeometry3D>();
            _Segment = new Dictionary<int, MeshGeometry3D>();
            _Cap = new Dictionary<int, MeshGeometry3D>();
            _SmallSmooth = new Dictionary<int, MeshGeometry3D>();
            _SmallFacet = new Dictionary<int, MeshGeometry3D>();
            _SmallSegment = new Dictionary<int, MeshGeometry3D>();
            _SmallCap = new Dictionary<int, MeshGeometry3D>();
            _SmallTrans = new Dictionary<int, Transform3D>();
            _SmallCapTrans = new Dictionary<int, Transform3D>();

            for (var _sx = MINSEG; _sx <= MAXSEG; _sx++)
            {
                // smooth mesh
                var _smooth = VolumetricMeshMaker.CylindricalSurface(2.4975d, 5d, _sx, 2, 225, 315);
                _smooth = _trans.Transform(_smooth);
                _smooth.Freeze();
                _Smooth.Add(_sx, _smooth);

                // faceted mesh
                var _facet = VolumetricMeshMaker.PolygonalCylindricalSurface(2.4975d, 5d, _sx, 2, 225, 315);
                _facet = _trans.Transform(_facet);
                _facet.Freeze();
                _Facet.Add(_sx, _facet);

                // segmented mesh
                var _segment = VolumetricMeshMaker.SegmentedCylindricalSurface(2.4975d, 5d, _sx, 2, 225, 315);
                _segment = _trans.Transform(_segment);
                _segment.Freeze();
                _Segment.Add(_sx, _segment);

                // cap model
                var _mb = new MeshBuilder();
                _mb.AddRevolvedGeometry(new[] { new Point(0d, 0d), new Point(0d, 2.4975d) }, new Point3D(2.5d, 2.5d, 0d), new Vector3D(0, 0, -1), (_sx * 4) + 1);
                _Cap.Add(_sx, _mb.ToMesh(true));

                // small smooth mesh
                _smooth = VolumetricMeshMaker.CylindricalSurface(1.24875d, 5d, _sx * 4, 2, 0, 360);
                _smooth.Freeze();
                _SmallSmooth.Add(_sx, _smooth);

                // small faceted mesh
                _facet = VolumetricMeshMaker.PolygonalCylindricalSurface(1.24875d, 5d, _sx * 4, 2, 0, 360);
                _facet.Freeze();
                _SmallFacet.Add(_sx, _facet);

                // small segmented mesh
                _segment = VolumetricMeshMaker.SegmentedCylindricalSurface(1.24875d, 5d, _sx * 4, 2, 0, 360);
                _segment.Freeze();
                _SmallSegment.Add(_sx, _segment);

                // small cap
                _mb = new MeshBuilder();
                _mb.AddRevolvedGeometry(new[] { new Point(0d, 0d), new Point(0d, 1.24875d) }, new Point3D(), new Vector3D(0, 0, -1), (_sx * 4) + 1);
                _SmallCap.Add(_sx, _mb.ToMesh(true));
            }

            // transforms
            foreach (var _a in AxisHelper.GetAll())
                foreach (var _p in new bool[] { false, true })
                    foreach (var _s in new bool[] { false, true })
                    {
                        var _grp = new Transform3DGroup();
                        switch (_a)
                        {
                            case Axis.Z:
                                // slide into place (XY)
                                _grp.Children.Add(new TranslateTransform3D(_p ? 3.75 : 1.25, _s ? 3.75 : 1.25, 0d));
                                break;
                            case Axis.Y:
                                // rotate into place
                                _grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));

                                // slide into place (ZX)
                                _grp.Children.Add(new TranslateTransform3D(_s ? 3.75 : 1.25, 0, _p ? 3.75 : 1.25));
                                break;
                            case Axis.X:
                            default:
                                // rotate into place
                                _grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90)));

                                // slide into place (YZ)
                                _grp.Children.Add(new TranslateTransform3D(0, _p ? 3.75 : 1.25, _s ? 3.75 : 1.25));
                                break;
                        }

                        _grp.Freeze();
                        _SmallTrans.Add(GetSmallTransformIndex(_a, _p, _s), _grp);
                    }

            // small cap transforms
            TranslateTransform3D _tt3(double x, double y)
            {
                var _move = new TranslateTransform3D(x, y, 0d);
                _move.Freeze();
                return _move;
            };
            _SmallCapTrans.Add(0, _tt3(1.25d, 1.25d));
            _SmallCapTrans.Add(1, _tt3(3.75d, 1.25d));
            _SmallCapTrans.Add(2, _tt3(1.25d, 3.75d));
            _SmallCapTrans.Add(3, _tt3(3.75d, 3.75d));
        }
        #endregion

        private static int GetSmallTransformIndex(Axis axis, bool pri, bool sec)
            => ((int)axis * 4) + (pri ? 2 : 0) + (sec ? 1 : 0);

        #region data
        private static Dictionary<int, MeshGeometry3D> _Smooth;
        private static Dictionary<int, MeshGeometry3D> _Facet;
        private static Dictionary<int, MeshGeometry3D> _Segment;
        private static Dictionary<int, MeshGeometry3D> _Cap;
        private static Dictionary<int, MeshGeometry3D> _SmallSmooth;
        private static Dictionary<int, MeshGeometry3D> _SmallFacet;
        private static Dictionary<int, MeshGeometry3D> _SmallSegment;
        private static Dictionary<int, MeshGeometry3D> _SmallCap;
        private static Dictionary<int, Transform3D> _SmallTrans;
        private static Dictionary<int, Transform3D> _SmallCapTrans;
        #endregion

        #region public static void AddCylindricalQuarterFace(...)
        public static void AddCylindricalQuarterFace(uint param, IPlusCellSpace cylinderSpace, BuildableGroup buildable,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            var _params = new CylinderParams(param);
            var _axis = _params.AnchorFace.GetAxis();
            int _segCount = _params.SegmentCount;
            _segCount = Math.Max(MINSEG, _segCount);
            _segCount = Math.Min(_segCount, MAXSEG);

            Func<BuildableMaterial> _material = () => cylinderSpace.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            var _bMesh = buildable.Context.GetBuildableMesh(cylinderSpace.GetBuildableMeshKey(face, effect), _material);

            var _rotPt = new Point3D(2.5d, 2.5d, 0d);
            if (face.GetAxis() == _axis)
            {
                // need an end-cap...
                _bMesh.AddMeshFace(_Cap[_segCount], new CellPosition(z, y, x), face, 45d, _rotPt);
            }
            else
            {
                // need a cylindrical side
                switch (face)
                {
                    case AnchorFace.XLow:
                    case AnchorFace.XHigh:
                    case AnchorFace.YLow:
                    case AnchorFace.YHigh:
                        {
                            var _rotate = (_axis != Axis.Z) ? 90d : 0d;
                            switch (_params.Style)
                            {
                                case CylinderStyle.Facet:
                                    _bMesh.AddMeshFace(_Facet[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                                case CylinderStyle.Segment:
                                    _bMesh.AddMeshFace(_Segment[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                                case CylinderStyle.Smooth:
                                default:
                                    _bMesh.AddMeshFace(_Smooth[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                            }
                        }
                        break;
                    case AnchorFace.ZLow:
                    case AnchorFace.ZHigh:
                        {
                            var _rotate = (_axis != Axis.Y) ? 90d : 0d;
                            switch (_params.Style)
                            {
                                case CylinderStyle.Facet:
                                    _bMesh.AddMeshFace(_Facet[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                                case CylinderStyle.Segment:
                                    _bMesh.AddMeshFace(_Segment[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                                case CylinderStyle.Smooth:
                                default:
                                    _bMesh.AddMeshFace(_Smooth[_segCount], new CellPosition(z, y, x), face, _rotate, _rotPt);
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region public static void AddSmallCylinder(uint param, ICellSpace cylinderSpace, BuildableGroup buildable, VisualEffect effect)
        public static void AddSmallCylinder(uint param, ICellSpace cylinderSpace,
            BuildableGroup buildable, VisualEffect effect)
        {
            if (cylinderSpace.IsInvisible)
                return;

            var _param = new WedgeParams(param);

            // get mesh prototype
            MeshGeometry3D _mesh = null;
            switch (_param.Style)
            {
                case CylinderStyle.Facet:
                    _mesh = _SmallFacet[_param.SegmentCount];
                    break;
                case CylinderStyle.Segment:
                    _mesh = _SmallSegment[_param.SegmentCount];
                    break;
                case CylinderStyle.Smooth:
                default:
                    _mesh = _SmallSmooth[_param.SegmentCount];
                    break;
            }

            // get material
            var _material = cylinderSpace.GetOtherFaceMaterial(0, effect);

            // geometry and transform
            var _pillar = new GeometryModel3D(_mesh, _material.Material)
            {
                Transform = _SmallTrans[GetSmallTransformIndex(_param.Axis, _param.InvertPrimary, _param.InvertSecondary)]
            };


            // freeze and add
            _pillar.Freeze();
            (_material.IsAlpha ? buildable.Alpha : buildable.Opaque).Children.Add(_pillar);
        }
        #endregion

        #region public static void AddSmallCylinderCap(uint param, IPlusCellSpace cylinderSpace, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public static void AddSmallCylinderCap(uint param, IPlusCellSpace cylinderSpace, BuildableGroup buildable,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            if (cylinderSpace.IsInvisible)
                return;

            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _segCount = _param.SegmentCount;
            if (_axis == face.GetAxis())
            {
                Func<BuildableMaterial> _material = () => cylinderSpace.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
                var _bMesh = buildable.Context.GetBuildableMesh(cylinderSpace.GetBuildableMeshKey(face, effect), _material);

                var _rotPt = new Point3D();
                if (face.GetAxis() == _axis)
                {
                    // need an end-cap...
                    Transform3D _getTrans(bool two, bool one)
                        => _SmallCapTrans[(two ? 2 : 0) + (one ? 1 : 0)];
                    Transform3D _move = null;
                    switch (face)
                    {
                        case AnchorFace.YLow:
                            _move = _getTrans(_param.InvertPrimary, _param.InvertSecondary);
                            break;
                        case AnchorFace.ZLow:
                        case AnchorFace.XLow:
                            _move = _getTrans(_param.InvertSecondary, !_param.InvertPrimary);
                            break;
                        case AnchorFace.YHigh:
                            _move = _getTrans(_param.InvertPrimary, !_param.InvertSecondary);
                            break;
                        case AnchorFace.ZHigh:
                        case AnchorFace.XHigh:
                        default:
                            _move = _getTrans(_param.InvertSecondary, _param.InvertPrimary);
                            break;
                    }
                    _bMesh.AddMeshFace(_SmallCap[_segCount], new CellPosition(z, y, x), face, 0d, _rotPt, _move);
                }
            }
        }
        #endregion
    }
}
