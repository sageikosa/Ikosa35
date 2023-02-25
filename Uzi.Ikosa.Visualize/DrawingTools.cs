using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace Uzi.Visualize
{
    public static class DrawingTools
    {
        #region data
        private static readonly Material _Highlighter = null;
        private static readonly MeshGeometry3D _Mesh;
        private static readonly MeshGeometry3D _LineMesh = null;
        #endregion

        public static Material Highlighter(Color color, double opacity)
        {
            var _highlighter = new DiffuseMaterial();
            var _brush = new SolidColorBrush(color)
            {
                Opacity = opacity
            };
            _highlighter.Brush = _brush;
            _highlighter.Freeze();
            return _highlighter;
        }

        #region Static Setup
        static DrawingTools()
        {
            // Highlighter
            _Highlighter = Highlighter(Colors.LightYellow, 0.37d);

            _Mesh = HedralGenerator.RectangularMesh(new Rect(0, 0, 5, 5), 1, 1, new Vector(5d, 5d));
            _Mesh.Freeze();

            // line points
            _LineMesh = new MeshGeometry3D();
            _LineMesh.Positions.Add(new Point3D(0, -0.04, -0.025));
            _LineMesh.Positions.Add(new Point3D(0, 0, 0.04));
            _LineMesh.Positions.Add(new Point3D(0, 0.04, -0.025));
            _LineMesh.Positions.Add(new Point3D(1, 0.04, 0.025));
            _LineMesh.Positions.Add(new Point3D(1, -0.04, 0.025));
            _LineMesh.Positions.Add(new Point3D(1, 0, -0.04));

            // texture mappings
            _LineMesh.TextureCoordinates.Add(new Point(0, 0));
            _LineMesh.TextureCoordinates.Add(new Point(0, 0.5));
            _LineMesh.TextureCoordinates.Add(new Point(0, 1));
            _LineMesh.TextureCoordinates.Add(new Point(1, 0));
            _LineMesh.TextureCoordinates.Add(new Point(1, 0.5));
            _LineMesh.TextureCoordinates.Add(new Point(1, 1));

            // Top-Bottom triangles
            _LineMesh.TriangleIndices.Add(0);
            _LineMesh.TriangleIndices.Add(1);
            _LineMesh.TriangleIndices.Add(2);
            _LineMesh.TriangleIndices.Add(3);
            _LineMesh.TriangleIndices.Add(4);
            _LineMesh.TriangleIndices.Add(5);

            // Top-end based sides
            _LineMesh.TriangleIndices.Add(4);
            _LineMesh.TriangleIndices.Add(0);
            _LineMesh.TriangleIndices.Add(5);
            _LineMesh.TriangleIndices.Add(1);
            _LineMesh.TriangleIndices.Add(4);
            _LineMesh.TriangleIndices.Add(3);
            _LineMesh.TriangleIndices.Add(2);
            _LineMesh.TriangleIndices.Add(3);
            _LineMesh.TriangleIndices.Add(5);

            // bottom-end based sides
            _LineMesh.TriangleIndices.Add(1);
            _LineMesh.TriangleIndices.Add(0);
            _LineMesh.TriangleIndices.Add(4);
            _LineMesh.TriangleIndices.Add(2);
            _LineMesh.TriangleIndices.Add(1);
            _LineMesh.TriangleIndices.Add(3);
            _LineMesh.TriangleIndices.Add(0);
            _LineMesh.TriangleIndices.Add(2);
            _LineMesh.TriangleIndices.Add(5);

            _LineMesh.Freeze();
        }
        #endregion

        public static Model3DGroup DebugGroup { get; set; }
        public static void DebugLine(Point3D pt1, Point3D pt2, Brush lineBrush)
        {
            if (DebugGroup != null)
            {
                DebugGroup.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (Action)(() => { DebugGroup.Children.Add(Line(pt1, pt2, lineBrush)); })
                    );
            }
        }

        public static Model3D CellGlow(int z, int y, int x)
            => CellGlow(z, y, x, _Highlighter);

        #region public static Model3D CellGlow(int z, int y, int x, System.Windows.Media.Media3D.Material brushMaterial)
        public static Model3D CellGlow(int z, int y, int x, System.Windows.Media.Media3D.Material brushMaterial)
        {
            var _model = new Model3DGroup();
            var _move = new TranslateTransform3D(x * 5d, y * 5d, z * 5d);
            _move.Freeze();

            // generate model, apply material, and move into place
            var _xMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _xMinusFace, HedralGenerator.XMTransform, _move);
            var _xPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _xPlusFace, HedralGenerator.XPTransform, _move);
            var _yMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _yMinusFace, HedralGenerator.YMTransform, _move);
            var _yPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _yPlusFace, HedralGenerator.YPTransform, _move);
            var _zMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _zMinusFace, HedralGenerator.ZMTransform, _move);
            var _zPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _zPlusFace, HedralGenerator.ZPTransform, _move);
            _model.Freeze();
            return _model;
        }
        #endregion

        public static Model3D CubeGlow(ICellLocation location, IGeometricSize size)
        {
            return CubeGlow(location, size, _Highlighter);
        }

        public static Model3D BoxEdges(IGeometricRegion region, Brush lineBrush, double width, double inset = 0d)
        {
            var _model3D = new Model3DGroup();
            if (region != null)
            {
                var _lx = region.LowerX * 5d + inset;
                var _ly = region.LowerY * 5d + inset;
                var _lz = region.LowerZ * 5d + inset;
                var _ux = (region.UpperX + 1) * 5d - inset;
                var _uy = (region.UpperY + 1) * 5d - inset;
                var _uz = (region.UpperZ + 1) * 5d - inset;

                _model3D.Children.Add(Line(new Point3D(_lx, _ly, _lz), new Point3D(_lx, _ly, _uz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_ux, _ly, _lz), new Point3D(_ux, _ly, _uz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_lx, _uy, _lz), new Point3D(_lx, _uy, _uz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_ux, _uy, _lz), new Point3D(_ux, _uy, _uz), lineBrush, width));

                _model3D.Children.Add(Line(new Point3D(_lx, _ly, _lz), new Point3D(_ux, _ly, _lz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_lx, _uy, _lz), new Point3D(_ux, _uy, _lz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_lx, _ly, _uz), new Point3D(_ux, _ly, _uz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_lx, _uy, _uz), new Point3D(_ux, _uy, _uz), lineBrush, width));

                _model3D.Children.Add(Line(new Point3D(_lx, _ly, _lz), new Point3D(_lx, _uy, _lz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_ux, _ly, _lz), new Point3D(_ux, _uy, _lz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_lx, _ly, _uz), new Point3D(_lx, _uy, _uz), lineBrush, width));
                _model3D.Children.Add(Line(new Point3D(_ux, _ly, _uz), new Point3D(_ux, _uy, _uz), lineBrush, width));
            }

            // freeze!
            _model3D.Freeze();
            return _model3D;
        }

        public static Model3D CrossHairs(Point3D pt, Brush lineBrush, double width, double length = 2d)
        {
            var _model3D = new Model3DGroup();
            var _x = pt.X;
            var _y = pt.Y;
            var _z = pt.Z;

            _model3D.Children.Add(Line(new Point3D(_x - length, _y, _z), new Point3D(_x + length, _y, _z), lineBrush, width));
            _model3D.Children.Add(Line(new Point3D(_x, _y - length, _z), new Point3D(_x, _y + length, _z), lineBrush, width));
            _model3D.Children.Add(Line(new Point3D(_x, _y, _z - length), new Point3D(_x, _y, _z + length), lineBrush, width));

            // freeze!
            _model3D.Freeze();
            return _model3D;
        }

        #region public static Model3D CubeGlow(ICellLocation location, IGeometricSize size, System.Windows.Media.Media3D.Material brushMaterial)
        public static Model3D CubeGlow(ICellLocation location, IGeometricSize size, System.Windows.Media.Media3D.Material brushMaterial)
        {
            var _model = new Model3DGroup();
            var _move = new TranslateTransform3D(location.X * 5d, location.Y * 5d, location.Z * 5d);
            _move.Freeze();

            // generate model, apply material, and move into place
            var _xMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _xMinusFace,
                new ScaleTransform3D(size.YExtent, size.ZExtent, 1, 5, 0, 0),
                HedralGenerator.XMTransform, _move);
            var _xPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _xPlusFace,
                new ScaleTransform3D(size.YExtent, size.ZExtent, 1),
                HedralGenerator.XPTransform, _move);
            var _yMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _yMinusFace,
                new ScaleTransform3D(size.XExtent, size.ZExtent, 1),
                HedralGenerator.YMTransform, _move);
            var _yPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _yPlusFace,
                new ScaleTransform3D(size.XExtent, size.ZExtent, 1, 5, 0, 0),
                HedralGenerator.YPTransform, _move);
            var _zMinusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _zMinusFace,
                new ScaleTransform3D(size.XExtent, size.YExtent, 1, 5, 0, 0),
                HedralGenerator.ZMTransform, _move);
            var _zPlusFace = new GeometryModel3D(_Mesh, brushMaterial);
            CellSpaceFaces.AddGeometry(_model, _zPlusFace,
                new ScaleTransform3D(size.XExtent, size.YExtent, 1),
                HedralGenerator.ZPTransform, _move);
            _model.Freeze();
            return _model;
        }
        #endregion

        #region public static Model3D Line(Point3D pt1, Point3D pt2, Brush lineBrush)
        public static Model3D Line(Point3D pt1, Point3D pt2, Brush lineBrush)
        {
            return Line(pt1, pt2, lineBrush, 1d);
        }
        #endregion

        #region public static Model3D Line(Point3D pt1, Point3D pt2, Brush lineBrush, double width)
        public static Model3D Line(Point3D pt1, Point3D pt2, Brush lineBrush, double width)
        {
            // regular and flat vectors
            Vector3D _v = pt2 - pt1;
            var _v2 = new Vector(_v.X, _v.Y);

            Model3D _line = new GeometryModel3D(_LineMesh, new DiffuseMaterial(lineBrush));

            // stretch, mutilate and toss it into place
            var _trans = new Transform3DGroup();
            _trans.Children.Add(new ScaleTransform3D(_v.Length, width, width));
            _trans.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), Math.Atan2(_v.Z, _v2.Length) * -180d / Math.PI), 0, 0, 0));
            _trans.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Math.Atan2(_v.Y, _v.X) * 180d / Math.PI), 0, 0, 0));
            _trans.Children.Add(new TranslateTransform3D(pt1.X, pt1.Y, pt1.Z));
            _trans.Freeze();
            _line.Transform = _trans;
            _line.Freeze();
            return _line;
        }
        #endregion

        #region public static LinearGradientBrush LineBrush(Color[] colors)
        /// <summary>Provides an evenly distributed LinearGradientBrush suitable for static Line method</summary>
        public static LinearGradientBrush LineBrush(Color[] colors)
        {
            var _brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            if ((colors != null) && (colors.Length > 1))
            {
                var _step = 1d / (colors.Length - 1d);
                for (var _cx = 0; _cx < colors.Length; _cx++)
                {
                    _brush.GradientStops.Add(new GradientStop(colors[_cx], _cx * _step));
                }
            }
            return _brush;
        }
        #endregion
    }
}
