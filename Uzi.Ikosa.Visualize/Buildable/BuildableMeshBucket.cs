using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class BuildableMeshBucket
    {
        public BuildableMeshBucket()
        {
            // default space for 256 points
            const int _POINTS = 256;
            _Points = new List<Point3D>(_POINTS);
            _Normals = new List<Vector3D>(_POINTS);
            _TextureCoords = new List<Point>(_POINTS);
            _TriIndexes = new List<int>(_POINTS * 4 / 3);
        }

        #region data
        private List<Point3D> _Points;
        private List<Vector3D> _Normals;
        private List<Point> _TextureCoords;
        private List<int> _TriIndexes;
        #endregion

        /// <summary>Count of points added</summary>
        public bool AtCapacity => _Points.Count >= 18000;  // guidelines recommend < 20K points ...

        #region public void AddCellFace(AnchorFace face, Vector3D[] bump)
        public void AddCellFace(AnchorFace face, params Vector3D[] bump)
        {
            // prepare!
            var _start = _Points.Count;

            // setup transform
            var _offset = new Vector3D();
            if ((bump?.Length ?? 0) > 0)
            {
                foreach (var _b in bump)
                    _offset += _b;
            }

            void _addPoint(Point3D pt)
            {
                _Points.Add(pt + _offset);
            };

            // add points
            switch (face)
            {
                case AnchorFace.XLow:
                    _addPoint(new Point3D(0, 5, 0));
                    _addPoint(new Point3D(0, 0, 0));
                    _addPoint(new Point3D(0, 0, 5));
                    _addPoint(new Point3D(0, 5, 5));
                    break;
                case AnchorFace.XHigh:
                    _addPoint(new Point3D(5, 0, 0));
                    _addPoint(new Point3D(5, 5, 0));
                    _addPoint(new Point3D(5, 5, 5));
                    _addPoint(new Point3D(5, 0, 5));
                    break;
                case AnchorFace.YLow:
                    _addPoint(new Point3D(0, 0, 0));
                    _addPoint(new Point3D(5, 0, 0));
                    _addPoint(new Point3D(5, 0, 5));
                    _addPoint(new Point3D(0, 0, 5));
                    break;
                case AnchorFace.YHigh:
                    _addPoint(new Point3D(5, 5, 0));
                    _addPoint(new Point3D(0, 5, 0));
                    _addPoint(new Point3D(0, 5, 5));
                    _addPoint(new Point3D(5, 5, 5));
                    break;
                case AnchorFace.ZLow:
                    _addPoint(new Point3D(5, 0, 0));
                    _addPoint(new Point3D(0, 0, 0));
                    _addPoint(new Point3D(0, 5, 0));
                    _addPoint(new Point3D(5, 5, 0));
                    break;
                case AnchorFace.ZHigh:
                default:
                    _addPoint(new Point3D(0, 0, 5));
                    _addPoint(new Point3D(5, 0, 5));
                    _addPoint(new Point3D(5, 5, 5));
                    _addPoint(new Point3D(0, 5, 5));
                    break;
            }

            // add normals
            var _norm = face.GetNormalVector();
            _Normals.Add(_norm);
            _Normals.Add(_norm);
            _Normals.Add(_norm);
            _Normals.Add(_norm);

            // add triangles
            _TriIndexes.Add(_start);
            _TriIndexes.Add(_start + 1);
            _TriIndexes.Add(_start + 2);

            _TriIndexes.Add(_start);
            _TriIndexes.Add(_start + 2);
            _TriIndexes.Add(_start + 3);

            // texture coordinates
            _TextureCoords.Add(new Point(0, 1));
            _TextureCoords.Add(new Point(1, 1));
            _TextureCoords.Add(new Point(1, 0));
            _TextureCoords.Add(new Point(0, 0));
        }
        #endregion

        #region public void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point uv0, Point uv1, Point uv2, Point uv3, params Vector3D[] bump)
        public void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point uv0, Point uv1, Point uv2, Point uv3, params Vector3D[] bump)
        {
            // setup transform
            var _offset = new Vector3D();
            if ((bump?.Length ?? 0) > 0)
            {
                foreach (var _b in bump)
                    _offset += _b;
            }

            // setup
            var _basePt = _Points.Count;
            var _norm = Vector3D.CrossProduct(p3 - p0, p1 - p0);
            _norm.Normalize();

            _Points.Add(p0);
            _Points.Add(p1);
            _Points.Add(p2);
            _Points.Add(p3);

            _TextureCoords.Add(uv0);
            _TextureCoords.Add(uv1);
            _TextureCoords.Add(uv2);
            _TextureCoords.Add(uv3);

            _Normals.Add(_norm);
            _Normals.Add(_norm);
            _Normals.Add(_norm);
            _Normals.Add(_norm);

            _TriIndexes.Add(_basePt + 0);
            _TriIndexes.Add(_basePt + 1);
            _TriIndexes.Add(_basePt + 2);

            _TriIndexes.Add(_basePt + 2);
            _TriIndexes.Add(_basePt + 3);
            _TriIndexes.Add(_basePt + 0);
        }
        #endregion

        #region public void AddMeshFace(MeshGeometry3D mesh, ICellLocation location, AnchorFace face, double rotate)
        public void AddMeshFace(MeshGeometry3D mesh, ICellLocation location, AnchorFace face, double rotate, Point3D rotatePoint, Transform3D fixup)
        {
            // prepare!
            var _start = _Points.Count;

            // transformation (location, rotation, etc...)
            var _transform = new Transform3DGroup();
            if (rotate != 0d)
                _transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotate), rotatePoint));
            if (fixup != null)
                _transform.Children.Add(fixup);
            _transform.Children.Add(face.Transform());
            _transform.Children.Add(new TranslateTransform3D(location.Vector3D()));

            void _addPoint(Point3D pt, Vector3D norm, Point txtr)
            {
                _Points.Add(_transform.Transform(pt));
                _Normals.Add(_transform.Transform(norm));
                _TextureCoords.Add(txtr);
            };

            // points and meta-data copy
            for (var _px = 0; _px < mesh.Positions.Count; _px++)
                _addPoint(mesh.Positions[_px], mesh.Normals[_px], mesh.TextureCoordinates[_px]);

            // triangle copy
            foreach (var _ti in mesh.TriangleIndices)
                _TriIndexes.Add(_ti + _start);
        }
        #endregion

        #region public void AddRectangularMesh(Rect rect, int xSteps, int ySteps, Vector textureSize, bool anchorTexture, AnchorFace face, params Vector3D[] bump)
        public void AddRectangularMesh(Rect rect, Vector textureSize, bool anchorTexture, AnchorFace face, bool forInner, params Vector3D[] bump)
        {
            // setup transform
            var _offset = new Vector3D();
            if ((bump?.Length ?? 0) > 0)
            {
                foreach (var _b in bump)
                    _offset += _b;
            }

            // setup
            var _basePt = _Points.Count;
            var _norm = face.GetNormalVector();

            // point adder
            void _addPt(Point pt)
            {
                var _pt = face.MapMeshPoint(pt, forInner);
                _Points.Add(_pt + _offset);
                _Normals.Add(_norm);
                _TextureCoords.Add(new Point(pt.X / textureSize.X, 1 - (pt.Y / textureSize.Y)));
            };

            // corners only (20% fewer points than center triangulation)
            _addPt(rect.TopLeft);
            _addPt(rect.TopRight);
            _addPt(rect.BottomRight);
            _addPt(rect.BottomLeft);

            // only two triangles (50% fewer triangles than center triangulation)
            _TriIndexes.Add(_basePt);
            _TriIndexes.Add(_basePt + 1);
            _TriIndexes.Add(_basePt + 2);

            _TriIndexes.Add(_basePt);
            _TriIndexes.Add(_basePt + 2);
            _TriIndexes.Add(_basePt + 3);

            if (anchorTexture)
            {
                // add texture anchor points
                var _pt = face.MapMeshPoint(new Point(), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(0d, 1d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(textureSize.X, 0), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(1d, 1d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(0, textureSize.Y), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(0d, 0d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(textureSize.X, textureSize.Y), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(1d, 0d));
                _Normals.Add(_norm);
            }
        }
        #endregion

        #region public void AddRightTriangularMesh(Rect footPrint, TriangleCorner corner, int xSteps, int ySteps, Vector textureSize, bool anchorTexture, Transform3D orient, params Transform3D[] bump)
        public void AddRightTriangularMesh(Rect footPrint, TriangleCorner corner,
            Vector textureSize, bool anchorTexture, AnchorFace face, bool forInner, params Vector3D[] bump)
        {
            // setup transform
            var _delta = new Point3D();
            if ((bump?.Length ?? 0) > 0)
            {
                foreach (var _t in bump)
                    _delta = _delta + _t;
            }
            var _offset = _delta - (new Point3D());

            // setup
            var _basePt = _Points.Count;
            var _norm = face.GetNormalVector();

            // NOTE: testing performance with reduced detail faces
            var xSteps = 1;
            var ySteps = 1;

            // NOTE: Rect is 2D and uses the left-handed coordinate system, so the Bottom is Y+Height (rather than Top)
            // point adder
            void _addPt(Point pt)
            {
                var _pt = face.MapMeshPoint(pt, forInner);
                _Points.Add(_pt + _offset);
                _Normals.Add(_norm);
                _TextureCoords.Add(new Point(pt.X / textureSize.X, 1 - (pt.Y / textureSize.Y)));
            };

            var _outerX = xSteps + 1;
            var _outerY = ySteps + 1;
            var _xDelta = footPrint.Width / xSteps;
            var _yDelta = footPrint.Height / ySteps;

            // build points for outer lattice
            //double _xPt = footPrint.X;
            var _xPt = 0d;
            if (footPrint.Width != 0)
            {
                var _slope = (footPrint.Height / footPrint.Width);
                for (var _x = 0; _x < _outerX; _x++)
                {
                    //double _yPt = footPrint.Y;
                    var _yPt = 0d;
                    for (var _y = 0; _y < _outerY; _y++)
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
                var _innerOffset = _outerX * _outerY;
                //_xPt = footPrint.X + (_xDelta / 2);
                _xPt = (_xDelta / 2);
                for (var _x = 0; _x < xSteps; _x++)
                {
                    var _xoIdx = _x * _outerY;
                    var _xiIdx = _x * ySteps + _innerOffset;
                    //double _yPt = footPrint.Y + (_yDelta / 2);
                    var _yPt = (_yDelta / 2);
                    for (var _y = 0; _y < ySteps; _y++)
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
                        _TriIndexes.Add(_basePt + _xoIdx + _y);
                        _TriIndexes.Add(_basePt + _xiIdx + _y);
                        _TriIndexes.Add(_basePt + _xoIdx + _y + 1);
                        // 1
                        _TriIndexes.Add(_basePt + _xoIdx + _y + 1);
                        _TriIndexes.Add(_basePt + _xiIdx + _y);
                        _TriIndexes.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                        // 2
                        _TriIndexes.Add(_basePt + _xoIdx + _y + 1 + _outerY);
                        _TriIndexes.Add(_basePt + _xiIdx + _y);
                        _TriIndexes.Add(_basePt + _xoIdx + _y + _outerY);
                        // 3
                        _TriIndexes.Add(_basePt + _xoIdx + _y + _outerY);
                        _TriIndexes.Add(_basePt + _xiIdx + _y);
                        _TriIndexes.Add(_basePt + _xoIdx + _y);
                        _yPt += _yDelta;
                    }
                    _xPt += _xDelta;
                }
            }

            if (anchorTexture)
            {
                // add texture anchor points
                var _pt = face.MapMeshPoint(new Point(), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(0d, 1d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(textureSize.X, 0), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(1d, 1d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(0, textureSize.Y), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(0d, 0d));
                _Normals.Add(_norm);

                _pt = face.MapMeshPoint(new Point(textureSize.X, textureSize.Y), forInner);
                _Points.Add(_pt + _offset);
                _TextureCoords.Add(new Point(1d, 0d));
                _Normals.Add(_norm);
            }
        }
        #endregion

        public Point3DCollection Points
            => new Point3DCollection(_Points);

        public Vector3DCollection Normals
            => new Vector3DCollection(_Normals);

        public PointCollection TextureCoordinates
            => new PointCollection(_TextureCoords);

        public Int32Collection TriangleIndexes
            => new Int32Collection(_TriIndexes);
    }
}
