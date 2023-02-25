using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Defines in-game context properties</summary>
    public interface IGeometricContext
    {
        MapContext MapContext { get; }

        /// <summary>LocalCellGroups in which the target has a presence.</summary>
        IEnumerable<LocalCellGroup> GetLocalCellGroups();

        /// <summary>Geometry of the target</summary>
        IGeometricRegion GeometricRegion { get; }

        /// <summary>Presence of Locator</summary>
        PlanarPresence PlanarPresence { get; }
    }

    public static class GeometricContextStatics
    {
        public static IEnumerable<SegmentSet> LinesToTarget(this IGeometricContext self, Point3D target,
            SegmentSetFactory factory, double? maxMultiDistance, PlanarPresence planar)
        {
            if (maxMultiDistance.HasValue)
            {
                // find closest, use it
                if (self.GeometricRegion.NearDistance(target) > maxMultiDistance)
                {
                    return from _cell in self.GeometricRegion.AllCellLocations()
                           select self.MapContext.Map.SegmentCells(_cell.GetPoint(), target, factory, planar);
                }
            }
            return from _pt in self.GeometricRegion.AllCorners()
                   select self.MapContext.Map.SegmentCells(_pt, target, factory, planar);
        }

        /// <summary>Provides all lines to all corners of target.</summary>
        public static IEnumerable<SegmentSet> LinesToTarget(this IGeometricContext self, IGeometricRegion target,
            SegmentSetFactory factory, double maxMultiDistance, PlanarPresence planar)
            => from _pt in target.AllCorners().Union(target.GetPoint3D().ToEnumerable())
               from _lSet in self.LinesToTarget(_pt, factory, maxMultiDistance, planar)
               select _lSet;

        /// <summary>Provides all lines to a set of points.  Used for room awareness.</summary>
        public static IEnumerable<SegmentSet> LinesToCells(this IGeometricContext self, List<Point3D> pts,
            SegmentSetFactory factory, double maxMultiDistance, PlanarPresence planar)
            => pts.SelectMany(_pt => self.LinesToTarget(_pt, factory, maxMultiDistance, planar));

        public static IEnumerable<SegmentSet> LinesToTarget(this IGeometricContext self, ICellLocation cell,
            Point3D target, SegmentSetFactory factory, PlanarPresence planar)
            => from _iSect in cell.AllCorners()
               select self.MapContext.Map.SegmentCells(_iSect, target, factory, planar);

        public static IEnumerable<SegmentSet> LinesToTarget(this IGeometricContext self, ICellLocation cell,
            IGeometricRegion target, SegmentSetFactory factory, PlanarPresence planar)
            => from _pt in target.AllCorners()
               from _lSet in self.LinesToTarget(cell, _pt, factory, planar)
               select _lSet;

        /// <summary>Provides all non-blocking lines from intersection</summary>
        /// <param name="target">point from which to determine line of effect</param>
        /// <returns>lines of effect to target</returns>
        /// <remarks>Special line-of-effect situations (such as water surface blocking fire spells, or anti-magic fields) are not considered.</remarks>
        public static IEnumerable<SegmentSet> EffectLinesToTarget(this IGeometricContext self, Point3D target,
            IGeometricRegion targetRegion, ITacticalInquiry[] exclusions, PlanarPresence planar)
            => self.LinesToTarget(target, new SegmentSetFactory(self.MapContext.Map, self.GeometricRegion, targetRegion,
                exclusions, SegmentSetProcess.Effect), null, planar).Where(_l => _l.IsLineOfEffect); // TODO: scale by range/power-level

        /// <summary>Provides all non-blocking lines to all corners of target.</summary>
        /// <param name="target">geometric extent for target</param>
        /// <returns>lines of effect, if any can be found</returns>
        /// <remarks>
        /// Special line-of-effect situations (such as water surface blocking fire spells, 
        /// or anti-magic fields) are not considered.
        /// </remarks>
        public static IEnumerable<SegmentSet> EffectLinesToTarget(this IGeometricContext self, IGeometricRegion target,
            ITacticalInquiry[] exclusions, PlanarPresence planar)
            => from _pt in target.AllCorners()
               from _lSet in self.EffectLinesToTarget(_pt, target, exclusions, planar)
               select _lSet;

        /// <summary>Examines lines from intersection until a divination line is found.</summary>
        /// <param name="source">point from which to determine line of detect</param>
        /// <returns>true if at least 1 line of detect can be found</returns>
        public static bool HasLineOfDetectFromSource(this IGeometricContext self, Point3D source,
            ITacticalInquiry[] exclusions, PlanarPresence planar)
            => self.DetectLinesFromSource(source, self.GeometricRegion, exclusions, planar).Any();

        public static IEnumerable<SegmentSet> LinesFromPoint(this IGeometricContext self, Point3D source,
            SegmentSetFactory factory, PlanarPresence planar)
            => from _iSect in self.GeometricRegion.AllCorners()
               select self.MapContext.Map.SegmentCells(source, _iSect, factory, planar);

        /// <summary>Examines lines from intersection until a non-blocking line is found.</summary>
        /// <param name="source">point from which to determine line of effect</param>
        /// <returns>true if at least 1 line of effect can be found</returns>
        /// <remarks>Special line-of-effect situations (such as water surface blocking fire spells, or anti-magic fields) are not considered.</remarks>
        public static bool HasLineOfEffectFromSource(this IGeometricContext self, Point3D source,
            IGeometricRegion targetRegion, ITacticalInquiry[] exclusions, PlanarPresence planar)
            => self.EffectLinesFromSource(source, targetRegion, exclusions, planar).Any();

        /// <summary>Provides all non-blocking lines from intersection</summary>
        /// <param name="target">point from which to determine line of effect</param>
        /// <returns>lines of effect to target</returns>
        /// <remarks>Special line-of-effect situations (such as water surface blocking fire spells, or anti-magic fields) are not considered.</remarks>
        public static IEnumerable<SegmentSet> EffectLinesFromSource(this IGeometricContext self, Point3D source,
            IGeometricRegion targetRegion, ITacticalInquiry[] exclusions, PlanarPresence planar)
            => from _lSet in self.LinesFromPoint(source,
                new SegmentSetFactory(self.MapContext.Map, self.GeometricRegion, targetRegion,
                    exclusions, SegmentSetProcess.Effect), planar)
               where _lSet.IsLineOfEffect
               select _lSet;

        /// <summary>Provides all divination lines from intersection</summary>
        /// <param name="target">point from which to determine line of detect</param>
        /// <returns>lines of detect to target</returns>
        public static IEnumerable<SegmentSet> DetectLinesFromSource(this IGeometricContext self, Point3D source,
            IGeometricRegion targetRegion, ITacticalInquiry[] exclusions, PlanarPresence planar)
            => from _lSet in self.LinesFromPoint(source, new SegmentSetFactory(self.MapContext.Map,
                self.GeometricRegion, targetRegion, exclusions, SegmentSetProcess.Detect), planar)
               where _lSet.IsLineOfDetect
               select _lSet;
    }
}
