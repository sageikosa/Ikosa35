using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Uzi.Visualize
{
    public static class HedralGenerator
    {
        private static MatrixTransform3D GetMatrixTransform(params Transform3D[] transforms)
        {
            var _group = new Transform3DGroup();
            foreach (var _t in transforms)
            {
                _group.Children.Add(_t);
            }

            var _matrix = new MatrixTransform3D(_group.Value);
            _matrix.Freeze();
            return _matrix;
        }

        #region construction
        static HedralGenerator()
        {
            // transforms
            _XMTransform = GetMatrixTransform(
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0d, 1d, 0d), -90)),
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), 90), 0d, 2.5d, 2.5d));
            _XPTransform = GetMatrixTransform(
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0d, 1d, 0d), 90), 5d, 2.5d, 0d),
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), 90), 5.0d, 2.5, 2.5));

            _YMFlip = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), 90));
            _YMFlip.Freeze();

            _YPTransform = GetMatrixTransform(
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), -90), 2.5d, 5d, 0d),
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0d, 1d, 0d), 180), 2.5d, 5d, 2.5d));

            _ZMFlip = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0d, 1d, 0d), 180), 2.5d, 2.5d, 0d);
            _ZMFlip.Freeze();

            _ZMAltFlip = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), 180), 2.5d, 2.5d, 0d);
            _ZMAltFlip.Freeze();
            _ZPBump = new TranslateTransform3D(0d, 0d, 5d);
            _ZPBump.Freeze();

            // flipped
            _FlipXMTransform = GetMatrixTransform(_ZMAltFlip, _XMTransform);
            _FlipXPTransform = GetMatrixTransform(_ZMAltFlip, _XPTransform);
            _FlipYMTransform = GetMatrixTransform(_ZMAltFlip, _YMFlip);
            _FlipYPTransform = GetMatrixTransform(_ZMAltFlip, _YPTransform);
            _FlipZMTransform = GetMatrixTransform(_ZMAltFlip, _ZMFlip);
            _FlipZPTransform = GetMatrixTransform(_ZMAltFlip, _ZPBump);
            _FlipXMTransform.Freeze();
            _FlipXPTransform.Freeze();
            _FlipYMTransform.Freeze();
            _FlipYPTransform.Freeze();
            _FlipZMTransform.Freeze();
            _FlipZPTransform.Freeze();

            _Null = new TranslateTransform3D();
            _Null.Freeze();
        }
        #endregion

        #region public static MeshGeometry3D RectangularMesh(Rect rect, int xSteps, int ySteps, Vector textureSize, bool anchorTexture=true, bool freeze = true, MeshGeometry3D geom = null)
        public static MeshGeometry3D RectangularMesh(Rect rect, int xSteps, int ySteps, Vector textureSize, bool anchorTexture = true, bool freeze = true, MeshGeometry3D geom = null)
        {
            var _basePt = geom?.Positions.Count ?? 0;
            var _norm = new Vector3D(0, 0, 1);

            // NOTE: testing performance with reduced detail faces
            xSteps = 1;
            ySteps = 1;
            var _ptCount =
                ((xSteps + 1) * (ySteps + 1))   // major
                + (xSteps * ySteps)             // minor (inner)
                + (anchorTexture ? 4 : 0);      // texture anchors

            // gather geometry data outside of a WPF geometry
            var _points = new List<Point3D>(_ptCount);
            var _normals = new List<Vector3D>(_ptCount);
            var _texture = new List<Point>(_ptCount);
            var _triangles = new List<int>((xSteps * ySteps) * 12);

            // point adder
            void _addPt(double x, double y)
            {
                _points.Add(new Point3D(x, y, 0));
                _normals.Add(_norm);
                _texture.Add(new Point(x / textureSize.X, 1 - (y / textureSize.Y)));
            };

            int _outerX = xSteps + 1;
            int _outerY = ySteps + 1;
            double _xDelta = rect.Width / xSteps;
            double _yDelta = rect.Height / ySteps;

            // build points for outer lattice
            double _xPt = rect.X;
            for (int _x = 0; _x < _outerX; _x++)
            {
                double _yPt = rect.Y;
                for (int _y = 0; _y < _outerY; _y++)
                {
                    // point and position facing out
                    _addPt(_xPt, _yPt);
                    _yPt += _yDelta;
                }
                _xPt += _xDelta;
            }

            // build points for inner lattice (and faces)
            int _innerOffset = _outerX * _outerY;
            _xPt = rect.X + (_xDelta / 2);
            for (int _x = 0; _x < xSteps; _x++)
            {
                int _xoIdx = _x * _outerY;
                int _xiIdx = _x * ySteps + _innerOffset;
                double _yPt = rect.Y + (_yDelta / 2);
                for (int _y = 0; _y < ySteps; _y++)
                {
                    _addPt(_xPt, _yPt);
                    // 0
                    _triangles.Add(_basePt + _xoIdx + _y);
                    _triangles.Add(_basePt + _xiIdx + _y);
                    _triangles.Add(_basePt + _xoIdx + _y + 1);
                    // 1
                    _triangles.Add(_basePt + _xoIdx + _y + 1);
                    _triangles.Add(_basePt + _xiIdx + _y);
                    _triangles.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                    // 2
                    _triangles.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                    _triangles.Add(_basePt + _xiIdx + _y);
                    _triangles.Add(_basePt + _xoIdx + _y + _outerY);
                    // 3
                    _triangles.Add(_basePt + _xoIdx + _y + _outerY);
                    _triangles.Add(_basePt + _xiIdx + _y);
                    _triangles.Add(_basePt + _xoIdx + _y);
                    _yPt += _yDelta;
                }
                _xPt += _xDelta;
            }

            if (anchorTexture)
            {
                // add texture anchor points
                _points.Add(new Point3D(0d, 0d, 0d));
                _texture.Add(new Point(0d, 1d));
                _normals.Add(_norm);

                _points.Add(new Point3D(textureSize.X, 0d, 0d));
                _texture.Add(new Point(1d, 1d));
                _normals.Add(_norm);

                _points.Add(new Point3D(0d, textureSize.Y, 0d));
                _texture.Add(new Point(0d, 0d));
                _normals.Add(_norm);

                _points.Add(new Point3D(textureSize.X, textureSize.Y, 0d));
                _texture.Add(new Point(1d, 0d));
                _normals.Add(_norm);
            }

            MeshGeometry3D _geom = null;
            if (geom == null)
            {
                // create new geometry with all data at once
                _geom = new MeshGeometry3D
                {
                    Positions = new Point3DCollection(_points),
                    Normals = new Vector3DCollection(_normals),
                    TextureCoordinates = new System.Windows.Media.PointCollection(_texture),
                    TriangleIndices = new System.Windows.Media.Int32Collection(_triangles)
                };
            }
            else
            {
                // create new geometry with all data at once, unioning existing geometry
                _geom = new MeshGeometry3D
                {
                    Positions = new Point3DCollection(geom.Positions.Union(_points)),
                    Normals = new Vector3DCollection(geom.Normals.Union(_normals)),
                    TextureCoordinates = new System.Windows.Media.PointCollection(geom.TextureCoordinates.Union(_texture)),
                    TriangleIndices = new System.Windows.Media.Int32Collection(geom.TriangleIndices.Union(_triangles))
                };
            }

            if (freeze)
            {
                _geom.Freeze();
            }

            return _geom;
        }
        #endregion

        #region public static MeshGeometry3D RightTriangularMesh(Rect footPrint, TriangleCorner corner, int xSteps, int ySteps, Vector textureSize, bool anchorTexture=true, bool freeze = true, MeshGeometry3D geom = null)
        public static MeshGeometry3D RightTriangularMesh(Rect footPrint, TriangleCorner corner, int xSteps, int ySteps, Vector textureSize, bool anchorTexture = true, bool freeze = true, MeshGeometry3D geom = null)
        {
            // NOTE: Rect is 2D and uses the left-handed coordinate system, so the Bottom is Y+Height (rather than Top)
            var _geom = (geom != null) ? geom : new MeshGeometry3D();
            var _basePt = _geom.Positions.Count;

            void _addPt(Point point)
            {
                _geom.Positions.Add(new Point3D(point.X, point.Y, 0));
                _geom.Normals.Add(new Vector3D(0d, 0d, 1d));
                _geom.TextureCoordinates.Add(new Point(point.X / textureSize.X, 1 - (point.Y / textureSize.Y)));
            };

            // NOTE: testing performance with reduced detail faces
            xSteps = 1;
            ySteps = 1;

            int _outerX = xSteps + 1;
            int _outerY = ySteps + 1;
            double _xDelta = footPrint.Width / xSteps;
            double _yDelta = footPrint.Height / ySteps;

            // build points for outer lattice
            //double _xPt = footPrint.X;
            double _xPt = 0;
            if (footPrint.Width != 0)
            {
                double _slope = (footPrint.Height / footPrint.Width);
                for (int _x = 0; _x < _outerX; _x++)
                {
                    //double _yPt = footPrint.Y;
                    double _yPt = 0;
                    for (int _y = 0; _y < _outerY; _y++)
                    {
                        // point and position facing out
                        Point _pt;
                        switch (corner)
                        {
                            case TriangleCorner.LowerLeft:
                                _pt = new Point(_xPt * (footPrint.Height - _yPt) / footPrint.Height + footPrint.X, _yPt + footPrint.Y);
                                break;
                            case TriangleCorner.UpperLeft:
                                _pt = new Point(_xPt * _yPt / footPrint.Height + footPrint.X, _yPt + footPrint.Y);
                                break;
                            case TriangleCorner.LowerRight:
                                _pt = new Point(_xPt + footPrint.X, (_yPt * _xPt / footPrint.Width) + footPrint.Y);
                                break;
                            case TriangleCorner.UpperRight:
                            default:
                                // kind of ugly, but the best I could hack out
                                _pt = new Point(_xPt + footPrint.X, (_yPt * _xPt / footPrint.Width) - (_slope * _xPt) + footPrint.Height + footPrint.Y);
                                break;
                        }
                        _addPt(_pt);
                        _yPt += _yDelta;
                    }
                    _xPt += _xDelta;
                }

                // build points for inner lattice (and faces)
                int _innerOffset = _outerX * _outerY;
                //_xPt = footPrint.X + (_xDelta / 2);
                _xPt = (_xDelta / 2);
                for (int _x = 0; _x < xSteps; _x++)
                {
                    int _xoIdx = _x * _outerY;
                    int _xiIdx = _x * ySteps + _innerOffset;
                    //double _yPt = footPrint.Y + (_yDelta / 2);
                    double _yPt = (_yDelta / 2);
                    for (int _y = 0; _y < ySteps; _y++)
                    {
                        Point _pt;
                        switch (corner)
                        {
                            case TriangleCorner.LowerLeft:
                                _pt = new Point(_xPt * (footPrint.Height - _yPt) / footPrint.Height + footPrint.X, _yPt + footPrint.Y);
                                break;
                            case TriangleCorner.UpperLeft:
                                _pt = new Point(_xPt * _yPt / footPrint.Height + footPrint.X, _yPt + footPrint.Y);
                                break;
                            case TriangleCorner.LowerRight:
                                _pt = new Point(_xPt + footPrint.X, (_yPt * _xPt / footPrint.Width) + footPrint.Y);
                                break;
                            case TriangleCorner.UpperRight:
                            default:
                                // kind of ugly, but the best I could hack out
                                _pt = new Point(_xPt + footPrint.X, (_yPt * _xPt / footPrint.Width) - (_slope * _xPt) + footPrint.Height + footPrint.Y);
                                break;

                        }
                        _addPt(_pt);
                        // 0
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y);
                        _geom.TriangleIndices.Add(_basePt + _xiIdx + _y);
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + 1);
                        // 1
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + 1);
                        _geom.TriangleIndices.Add(_basePt + _xiIdx + _y);
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                        // 2
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                        _geom.TriangleIndices.Add(_basePt + _xiIdx + _y);
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + _outerY);
                        // 3
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y + _outerY);
                        _geom.TriangleIndices.Add(_basePt + _xiIdx + _y);
                        _geom.TriangleIndices.Add(_basePt + _xoIdx + _y);
                        _yPt += _yDelta;
                    }
                    _xPt += _xDelta;
                }
            }

            if (anchorTexture)
            {
                // add texture anchor points
                _geom.Positions.Add(new Point3D(0d, 0d, 0d));
                _geom.TextureCoordinates.Add(new Point(0d, 1d));

                _geom.Positions.Add(new Point3D(textureSize.X, 0d, 0d));
                _geom.TextureCoordinates.Add(new Point(1d, 1d));

                _geom.Positions.Add(new Point3D(0d, textureSize.Y, 0d));
                _geom.TextureCoordinates.Add(new Point(0d, 0d));

                _geom.Positions.Add(new Point3D(textureSize.X, textureSize.Y, 0d));
                _geom.TextureCoordinates.Add(new Point(1d, 0d));
            }
            if (freeze)
            {
                _geom.Freeze();
            }

            return _geom;
        }
        #endregion

        public static MeshGeometry3D CircularMesh(Point center, double radius, double startAngle, double endAngle, int segments, Vector textureSize, bool anchorTexture = true, MeshGeometry3D geom = null)
        {
            var _geom = (geom != null) ? geom : new MeshGeometry3D();
            var _basePt = _geom.Positions.Count;

            void _addPt(Point point)
            {
                _geom.Positions.Add(new Point3D(point.X, point.Y, 0));
                _geom.Normals.Add(new Vector3D(0d, 0d, 1d));
                _geom.TextureCoordinates.Add(new Point(point.X / textureSize.X, 1 - (point.Y / textureSize.Y)));
            };

            // center
            _addPt(center);

            // start point (via translation)
            // TODO:

            // generate points (via rotation)
            // TODO:

            if (anchorTexture)
            {
                // add texture anchor points
                _geom.Positions.Add(new Point3D(0d, 0d, 0d));
                _geom.TextureCoordinates.Add(new Point(0d, 1d));

                _geom.Positions.Add(new Point3D(textureSize.X, 0d, 0d));
                _geom.TextureCoordinates.Add(new Point(1d, 1d));

                _geom.Positions.Add(new Point3D(0d, textureSize.Y, 0d));
                _geom.TextureCoordinates.Add(new Point(0d, 0d));

                _geom.Positions.Add(new Point3D(textureSize.X, textureSize.Y, 0d));
                _geom.TextureCoordinates.Add(new Point(1d, 0d));
            }
            _geom.Freeze();
            return _geom;
        }

        // TODO: generate points on circular path
        // TODO: generate fan from path (assumes relatively convex to the starting point)

        #region public static GeometryModel3D GeometryModel3DTransform(MeshGeometry3D mesh, Material faceMaterial, params Transform3D[] trans)
        public static GeometryModel3D GeometryModel3DTransform(MeshGeometry3D mesh, Material faceMaterial, params Transform3D[] trans)
        {
            var _faceModel = new GeometryModel3D(mesh, faceMaterial);

            // build transform
            var _tGroup = new Transform3DGroup();
            foreach (var _trans in trans)
            {
                if (_trans != null)
                {
                    _tGroup.Children.Add(_trans);
                }
            }
            _tGroup.Freeze();

            // transform and return
            _faceModel.Transform = _tGroup;
            _faceModel.Freeze();
            return _faceModel;
        }
        #endregion

        #region Predefined Transforms for Cube Construction
        private static MatrixTransform3D _XMTransform = null;
        private static MatrixTransform3D _XPTransform = null;
        private static RotateTransform3D _YMFlip = null;
        private static MatrixTransform3D _YPTransform = null;

        /// <summary>Flips over ZM along a central Y line</summary>
        private static RotateTransform3D _ZMFlip = null;
        /// <summary>Flips over ZM along a central X line</summary>
        private static RotateTransform3D _ZMAltFlip = null;
        private static TranslateTransform3D _ZPBump = null;
        private static Transform3D _Null = null;

        private static MatrixTransform3D _FlipXMTransform = null;
        private static MatrixTransform3D _FlipXPTransform = null;
        private static MatrixTransform3D _FlipYMTransform = null;
        private static MatrixTransform3D _FlipYPTransform = null;

        /// <summary>Flips over ZM along a central Y line</summary>
        private static MatrixTransform3D _FlipZMTransform = null;
        private static MatrixTransform3D _FlipZPTransform = null;

        #endregion

        // TODO: merge sets of face transforms into single matrix transforms
        public static Transform3D XMTransform => _XMTransform;
        public static Transform3D XPTransform => _XPTransform;
        public static Transform3D YMTransform => _YMFlip;
        public static Transform3D YPTransform => _YPTransform;
        /// <summary>Flips over ZM along a central Y line</summary>
        public static Transform3D ZMTransform => _ZMFlip;
        /// <summary>Flips over ZM along a central X line</summary>
        public static Transform3D ZMAltFlip => _ZMAltFlip;
        public static Transform3D ZPTransform => _ZPBump;
        public static Transform3D NullTransform => _Null;

        public static Transform3D FlippedXMTransform => _FlipXMTransform;
        public static Transform3D FlippedXPTransform => _FlipXPTransform;
        public static Transform3D FlippedYMTransform => _FlipYMTransform;
        public static Transform3D FlippedYPTransform => _FlipYPTransform;
        public static Transform3D FlippedZMTransform => _FlipZMTransform;
        public static Transform3D FlippedZPTransform => _FlipZPTransform;
    }
}
