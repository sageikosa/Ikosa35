using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>Location and size</summary>
    public interface IGeometricRegion
    {
        int LowerZ { get; }
        int LowerY { get; }
        int LowerX { get; }
        int UpperZ { get; }
        int UpperY { get; }
        int UpperX { get; }
        Point3D GetPoint3D();
        IEnumerable<ICellLocation> AllCellLocations();
        bool ContainsCell(ICellLocation location);
        bool ContainsCell(int z, int y, int x);
        IGeometricRegion Move(ICellLocation offset);
        bool IsCellAtSurface(ICellLocation location, AnchorFace surfaceFace);
    }

    public static class IGeometricRegionHelper
    {
        public static string ToCubicInfoString(this IGeometricRegion self)
            => $@"LowerZYX({self?.LowerZ ?? double.NaN}, {self?.LowerY ?? double.NaN}, {self?.LowerX ?? double.NaN})...UpperZYX({self?.UpperZ ?? double.NaN}, {self?.UpperY ?? double.NaN}, {self?.UpperX ?? double.NaN})";

        /// <summary>determines if other region is on same side, other side, or stradling the boundary determined by the face</summary>
        public static BoundarySide GetBoundarySide(this IGeometricRegion self, IGeometricRegion other, AnchorFace face)
        {
            if ((self != null) && (other != null))
                switch (face)
                {
                    case AnchorFace.XLow:
                        if (other.UpperX < self.LowerX)
                            return BoundarySide.Other;
                        if (other.LowerX < self.LowerX)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;

                    case AnchorFace.XHigh:
                        if (other.LowerX > self.UpperX)
                            return BoundarySide.Other;
                        if (other.UpperX > self.UpperX)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;

                    case AnchorFace.YLow:
                        if (other.UpperY < self.LowerY)
                            return BoundarySide.Other;
                        if (other.LowerY < self.LowerY)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;
                    case AnchorFace.YHigh:
                        if (other.LowerY > self.UpperY)
                            return BoundarySide.Other;
                        if (other.UpperY > self.UpperY)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;

                    case AnchorFace.ZLow:
                        if (other.UpperZ < self.LowerZ)
                            return BoundarySide.Other;
                        if (other.LowerZ < self.LowerZ)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;

                    case AnchorFace.ZHigh:
                    default:
                        if (other.LowerZ > self.UpperZ)
                            return BoundarySide.Other;
                        if (other.UpperZ > self.UpperZ)
                            return BoundarySide.Stradle;
                        return BoundarySide.Same;
                }
            return BoundarySide.Stradle;
        }

        public static bool HasTargetAccess(this IGeometricRegion self, AnchorFace boundary, IGeometricRegion source, IGeometricRegion target)
        {
            var _source = self.GetBoundarySide(source, boundary);
            if (_source == BoundarySide.Stradle)
                return true;
            var _target = self.GetBoundarySide(target, boundary);
            if (_target == BoundarySide.Stradle)
                return true;
            return _source == _target;
        }
    }
}
