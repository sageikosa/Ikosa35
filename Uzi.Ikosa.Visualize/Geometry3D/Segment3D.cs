using System;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>
    /// Points, Vector, and Bounding Rectangle
    /// </summary>
    public struct Segment3D
    {
        public Segment3D(Point3D p1, Point3D p2)
        {
            _Point1 = p1;
            _Point2 = p2;
            _Vector = p2 - p1;
            var _start = new Point3D(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Min(p1.Z, p2.Z));
            var _end = new Point3D(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), Math.Max(p1.Z, p2.Z));
            var _size = _end - _start;
            _Rect3D = new Rect3D(_start, new Size3D(_size.X, _size.Y, _size.Z));
        }

        #region data
        private Point3D _Point1;
        private Point3D _Point2;
        private Vector3D _Vector;
        private Rect3D _Rect3D;
        #endregion

        public Point3D Point1 => _Point1;
        public Point3D Point2 => _Point2;
        public Vector3D Vector => _Vector;
        public Rect3D Rect3D => _Rect3D;

        /// <summary>True if this segment is within the bounds of the test segment</summary>
        public bool WithinBoundsOf(Segment3D test)
            => test.Rect3D.Contains(Point1) && test.Rect3D.Contains(Point2);
    }
}
