using System;
using System.Windows;

namespace Uzi.Visualize
{
    [Serializable]
    public class Cylinder : Sphere
    {
        public Cylinder(ICellLocation origin, LocationAimMode locMode, int radiusInCells, int depth)
            : base(origin, locMode, radiusInCells)
        {
            _Depth = depth;
            _ScaleDepth = _Depth * 5d;
            _ScaleDiscCenter = new Point(_ScaleOrigin.X, _ScaleOrigin.Y);
        }

        #region data
        private int _Depth;
        private double _ScaleDepth;
        private Point _ScaleDiscCenter;
        #endregion

        public override int LowerZ => _Origin.Z - _Depth;
        public override int UpperZ => _Origin.Z;

        protected override bool IsIntersectInGeometry(int z, int y, int x)
        {
            if ((LowerZ <= z) && (z <= UpperZ))
            {
                return ((new Point(x * 5d, y * 5d)) - _ScaleDiscCenter).LengthSquared <= _SquaredRadius;
            }
            else
            {
                return false;
            }
        }

        public override IGeometricRegion Move(ICellLocation offset)
            => new Cylinder(_Origin.Add(offset), _LocMode, _Radius, _Depth);
    }

    [Serializable]
    public class CylinderBuilder : IGeometryBuilder
    {
        public CylinderBuilder(int radius, int depth)
        {
            _Radius = radius;
            _Depth = depth;
        }

        #region data
        private int _Radius;
        private int _Depth;
        #endregion

        public int Radius
        {
            get => _Radius;
            set => _Radius = value;
        }

        public int Depth
        {
            get => _Depth;
            set => _Depth = value;
        }

        // IGeometryBuilder Members
        public IGeometricRegion BuildGeometry(LocationAimMode locMode, ICellLocation location)
            => new Cylinder(location, locMode, _Radius, _Depth);
    }
}
