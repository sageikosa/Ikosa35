using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Core
{
    public static class GeometricHelper
    {
        #region public static IEnumerable<Intersection> AllIntersections(this IGeometricRegion source)
        /// <summary>Gets all distinct intersections of the target</summary>
        public static IEnumerable<Intersection> AllIntersections(this IGeometricRegion source)
        {
            return (from _cell in source.AllCellLocations()
                    from _i in _cell.AllIntersections()
                    select _i).Distinct();
        }
        #endregion

        #region public static IEnumerable<Intersection> AllIntersections(this ICellLocation self)
        public static IEnumerable<Intersection> AllIntersections(this ICellLocation self)
        {
            yield return new Intersection(self.Z, self.Y, self.X);
            yield return new Intersection(self.Z, self.Y, self.X + 1);
            yield return new Intersection(self.Z, self.Y + 1, self.X);
            yield return new Intersection(self.Z, self.Y + 1, self.X + 1);
            yield return new Intersection(self.Z + 1, self.Y, self.X);
            yield return new Intersection(self.Z + 1, self.Y, self.X + 1);
            yield return new Intersection(self.Z + 1, self.Y + 1, self.X);
            yield return new Intersection(self.Z + 1, self.Y + 1, self.X + 1);
            yield break;
        }
        #endregion

        public static bool ContainsIntersection(this IGeometricRegion region, Intersection intersect)
        {
            return region.AllIntersections().Any(_i => _i == intersect);
        }
    }
}
