using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public static class IGeometricHelper
    {
        public static ICellLocation GetExitCubicOffset(this IGeometricRegion self, ICellLocation cell, IGeometricSize maxSize)
        {
            var _z = Math.Min((int)maxSize.ZHeight, self.StepsToExit(cell, new CellPosition(-1, 0, 0)));
            var _y = Math.Min((int)maxSize.YLength, self.StepsToExit(cell, new CellPosition(0, -1, 0)));
            var _x = Math.Min((int)maxSize.XLength, self.StepsToExit(cell, new CellPosition(0, 0, -1)));
            return new CellPosition(1 - _z, 1 - _y, 1 - _x);
        }

        /// <summary>Returns number of steps needed for the cell to exit the region in the specified direction</summary>
        public static int StepsToExit(this IGeometricRegion self, ICellLocation cell, ICellLocation offset)
        {
            var _steps = 0;
            var _loc = cell;
            while (self.ContainsCell(_loc) && (_steps < int.MaxValue))
            {
                _steps++;
                _loc = _loc.Add(offset);
            }
            return _steps;
        }

        public static bool IsEquivalent(this IGeometricRegion self, IGeometricRegion geomExtent)
            => (geomExtent?.AllCellLocations().All(_l => self?.ContainsCell(_l) ?? false) ?? false)
            && (self?.AllCellLocations().All(_l => geomExtent?.ContainsCell(_l) ?? false) ?? false);

        /// <summary>True if the geometric regions overlap</summary>
        public static bool ContainsGeometricRegion(this IGeometricRegion self, IGeometricRegion geomExtent)
            => geomExtent.AllCellLocations().Any(_l => self?.ContainsCell(_l) ?? false);

        /// <summary>Returns the point at the cell boundary intersection.  Intersection (0,0,0) is the origin.</summary>
        public static Point3D Point3D(this ICellLocation self)
            => new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d);

        /// <summary>Returns the vector to the cell boundary intersection.  Intersection (0,0,0) is the origin.</summary>
        public static Vector3D Vector3D(this ICellLocation self)
            => new Vector3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d);

        /// <summary>Distance between two cell centroids</summary>
        public static double Distance(int z1, int y1, int x1, int z2, int y2, int x2)
            => Distance(GetPoint(z1, y1, x1), z2, y2, x2);

        /// <summary>Point at a cell centroid</summary>
        public static Point3D GetPoint(int z, int y, int x)
            => new Point3D((x * 5d) + 2.5d, (y * 5d) + 2.5d, (z * 5d) + 2.5d);

        /// <summary>Point at a cell centroid</summary>
        public static Point3D GetPoint(this ICellLocation self)
            => GetPoint(self.Z, self.Y, self.X);

        public static Point3D GetCellSnapPoint(this ICellLocation self, ICellLocation target)
        {
            double _offSet(double src, double trg)
                => src < trg ? 0d : src == trg ? 2.5d : 5d;
            return new Point3D(self.X * 5d + _offSet(self.X, target.X), self.Y * 5d + _offSet(self.Y, target.Y), self.Z * 5d + _offSet(self.Z, target.Z));
        }

        /// <summary>Distance from a point to a cell centroid</summary>
        public static double Distance(Point3D p1, int z2, int y2, int x2)
            => Math.Abs((GetPoint(z2, y2, x2) - p1).Length);

        /// <summary>Distance from a point to a cell centroid</summary>
        public static double Distance(Point3D p1, ICellLocation location)
            => Distance(p1, location.Z, location.Y, location.X);

        /// <summary>Distance between two cell centroids</summary>
        public static double Distance(ICellLocation l1, ICellLocation l2)
            => Distance(l1.Z, l1.Y, l1.X, l2.Z, l2.Y, l2.X);

        public static CellPosition Subtract(this ICellLocation origin, ICellLocation destination)
            => new CellPosition(destination.Z - origin.Z, destination.Y - origin.Y, destination.X - origin.X);

        #region public static CellPosition Add(this ICellLocation origin, params AnchorFace[] offsets)
        /// <summary>Returns the ICellLocation specified by the provided AnchorFaces</summary>
        public static CellPosition Add(this ICellLocation origin, params AnchorFace[] offsets)
        {
            var _z = origin.Z;
            var _y = origin.Y;
            var _x = origin.X;
            if (offsets.Contains(AnchorFace.ZHigh))
                _z++;
            else if (offsets.Contains(AnchorFace.ZLow))
                _z--;
            if (offsets.Contains(AnchorFace.YHigh))
                _y++;
            else if (offsets.Contains(AnchorFace.YLow))
                _y--;
            if (offsets.Contains(AnchorFace.XHigh))
                _x++;
            else if (offsets.Contains(AnchorFace.XLow))
                _x--;
            return new CellPosition(_z, _y, _x);
        }
        #endregion

        #region public static ICellLocation Add(this ICellLocation origin, AnchorFaceList offsets)
        /// <summary>Returns the ICellLocation specified by the AnchorFaceList</summary>
        public static ICellLocation Add(this ICellLocation origin, AnchorFaceList offsets)
        {
            var _z = origin.Z;
            var _y = origin.Y;
            var _x = origin.X;
            if (offsets.Contains(AnchorFace.ZHigh))
                _z++;
            else if (offsets.Contains(AnchorFace.ZLow))
                _z--;
            if (offsets.Contains(AnchorFace.YHigh))
                _y++;
            else if (offsets.Contains(AnchorFace.YLow))
                _y--;
            if (offsets.Contains(AnchorFace.XHigh))
                _x++;
            else if (offsets.Contains(AnchorFace.XLow))
                _x--;
            return new CellPosition(_z, _y, _x);
        }
        #endregion

        /// <summary>Returns the ICellLocation specified by an offset location</summary>
        public static ICellLocation Add(this ICellLocation origin, ICellLocation offset)
            => new CellPosition(origin.Z + offset.Z, origin.Y + offset.Y, origin.X + offset.X);

        /// <summary>True if moving from the specified cell in the given direction exits the region</summary>
        public static bool IsCellUnboundAtFace(this IGeometricRegion self, ICellLocation location, AnchorFace face)
            => !self.ContainsCell(location.Add(face));

        #region public static double NearDistanceToCell(this IGeometricRegion self, ICellLocation cell)
        public static double NearDistanceToCell(this IGeometricRegion self, ICellLocation cell)
        {
            if (cell != null)
            {
                double _separation(int lSrc, int uSrc, int lTrg, int uTrg)
                {
                    if (lSrc > uTrg)
                        return lSrc - uTrg;
                    if (lTrg > uSrc)
                        return lTrg - uSrc;
                    return 0;
                };
                var _z = _separation(self.LowerZ, self.UpperZ, cell.Z, cell.Z + 1) * 5;
                var _y = _separation(self.LowerY, self.UpperY, cell.Y, cell.Y + 1) * 5;
                var _x = _separation(self.LowerX, self.UpperX, cell.X, cell.X + 1) * 5;
                return Math.Sqrt(_z * _z + _y * _y + _x * _x);
            }
            return double.MaxValue;
        }
        #endregion

        #region public static double NearDistance(this IGeometricRegion source,IGeometricRegion target)
        /// <summary>
        /// Gets the minimum distance between two IGeometricRegions (cell based)
        /// </summary>
        /// <returns>Absolute value of distance, or double.MaxValue if target not found</returns>
        public static double NearDistance(this IGeometricRegion source, IGeometricRegion target)
        {
            if (target != null)
            {
                double _separation(int lSrc, int uSrc, int lTrg, int uTrg)
                {
                    if (lSrc > uTrg)
                        return lSrc - uTrg;
                    if (lTrg > uSrc)
                        return lTrg - uSrc;
                    return 0;
                };
                var _z = _separation(source.LowerZ, source.UpperZ, target.LowerZ, target.UpperZ) * 5;
                var _y = _separation(source.LowerY, source.UpperY, target.LowerY, target.UpperY) * 5;
                var _x = _separation(source.LowerX, source.UpperX, target.LowerX, target.UpperX) * 5;
                return Math.Sqrt(_z * _z + _y * _y + _x * _x);
            }
            return double.MaxValue;
        }
        #endregion

        /// <summary>Gets the minimum distance between two IGeometricRegions (cell based)</summary>
        /// <returns>Absolute value of distance, or double.MaxValue if target not found</returns>
        public static Point3D NearIntersection(this IGeometricRegion source, IGeometricRegion target)
            => (from _pt in target.AllCorners()
                let _intersect = source.NearIntersection(_pt)
                let _dist = (_pt - _intersect).Length
                orderby _dist
                select _intersect).FirstOrDefault();

        /// <summary>Gets all distinct intersections of the target</summary>
        public static IEnumerable<Point3D> AllCorners(this IGeometricRegion source)
            => (from _cell in source.AllCellLocations()
                from _i in _cell.AllCorners()
                select _i).Distinct();

        /// <summary>Gets the nearest intersection to an intersection</summary>
        /// <returns>Absolute value of distance</returns>
        public static Point3D NearIntersection(this IGeometricRegion source, Point3D point)
            => (from _pt in source.AllCorners()
                let _dist = (point - _pt).Length
                orderby _dist
                select _pt).FirstOrDefault();

        #region public static IEnumerable<Point3D> AllCorners(this ICellLocation self)
        /// <summary>Yields all corners of a single cell</summary>
        public static IEnumerable<Point3D> AllCorners(this ICellLocation self)
        {
            Point3D _off(CellPosition src, double x, double y, double z)
            {
                var _pt = src.Point3D();
                //_pt.Offset(x, y, z);
                return _pt;
            };

            yield return _off(new CellPosition(self.Z, self.Y, self.X), 0.0025d, 0.0025d, 0.0025d);
            yield return _off(new CellPosition(self.Z, self.Y, self.X + 1), -0.0025d, 0.0025d, 0.0025d);
            yield return _off(new CellPosition(self.Z, self.Y + 1, self.X), 0.0025d, -0.0025d, 0.0025d);
            yield return _off(new CellPosition(self.Z, self.Y + 1, self.X + 1), -0.0025d, -0.0025d, 0.0025d);
            yield return _off(new CellPosition(self.Z + 1, self.Y, self.X), 0.0025d, 0.0025d, -0.0025d);
            yield return _off(new CellPosition(self.Z + 1, self.Y, self.X + 1), -0.0025d, 0.0025d, -0.0025d);
            yield return _off(new CellPosition(self.Z + 1, self.Y + 1, self.X), 0.0025d, -0.0025d, -0.0025d);
            yield return _off(new CellPosition(self.Z + 1, self.Y + 1, self.X + 1), -0.0025d, -0.0025d, -0.0025d);
            yield break;
        }
        #endregion

        public static IEnumerable<Point3D> FaceCorners(this ICellLocation self, AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.XHigh:
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    break;

                case AnchorFace.XLow:
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    break;

                case AnchorFace.YHigh:
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    break;

                case AnchorFace.YLow:
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d + 5);
                    break;

                case AnchorFace.ZHigh:
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d + 5);
                    break;

                case AnchorFace.ZLow:
                default:
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d, self.Y * 5.0d + 5, self.Z * 5.0d);
                    yield return new Point3D(self.X * 5.0d + 5, self.Y * 5.0d + 5, self.Z * 5.0d);
                    break;
            }
            yield break;
        }

        /// <summary>
        /// true if location within 1 cell in any direction from this (including same cell)
        /// </summary>
        public static bool IsCellTouchingBounds(this IGeometricRegion self, ICellLocation location)
            => location == null
            ? false
            : (new Cubic(self.LowerZ - 1, self.LowerY - 1, self.LowerX - 1, self.UpperZ + 1, self.UpperY + 1, self.UpperX + 1)).ContainsCell(location);

        /// <summary>Gets the smallest bounding cube containing both regions</summary>
        public static Cubic ContainingCube(this IGeometricRegion region1, IGeometricRegion region2)
            => new Cubic(
                Math.Min(region1.LowerZ, region2.LowerZ),
                Math.Min(region1.LowerY, region2.LowerY),
                Math.Min(region1.LowerX, region2.LowerX),
                Math.Max(region1.UpperZ, region2.UpperZ),
                Math.Max(region1.UpperY, region2.UpperY),
                Math.Max(region1.UpperX, region2.UpperX));

        /// <summary>Gets the minimum distance to a point</summary>
        /// <returns>Absolute value of distance</returns>
        public static double NearDistance(this IGeometricRegion source, Point3D point)
            => source?.AllCorners()
            .Select(_pt => (point - _pt).Length)
            .Min() ?? double.MaxValue;
    }
}
