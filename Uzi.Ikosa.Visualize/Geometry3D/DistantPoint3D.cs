using System;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Geometry3D
{
    public readonly struct DistantPoint3D
    {
        public DistantPoint3D(Point3D point, Point3D origin)
        {
            Distance = (point - origin).Length;
            Point3D = point;
            IsValid = true;
        }

        public DistantPoint3D(bool invalid)
        {
            Distance = Double.MaxValue;
            Point3D = new Point3D();
            IsValid = false;
        }

        public readonly double Distance;
        public readonly Point3D Point3D;
        public readonly bool IsValid;
    }
}
