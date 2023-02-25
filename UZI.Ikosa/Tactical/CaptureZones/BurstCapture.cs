using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Burst capture finds absolute locators in region, regardless of line of effect.</summary>
    [Serializable]
    public class BurstCapture : CaptureZone
    {
        public BurstCapture(MapContext mapContext, object source, Geometry geometry,
            Intersection origin, IBurstCaptureCapable burstBack, PlanarPresence planar)
            : base(mapContext, source, geometry, planar, origin)
        {
            _BurstBack = burstBack;
        }

        #region private data
        private IBurstCaptureCapable _BurstBack;
        #endregion

        protected override void OnRemoveZone()
        {
            // no permanent zone
        }

        /// <summary>called to Capture</summary>
        public IEnumerable<CoreStep> DoBurst()
            => from _loc in _BurstBack.ProcessOrder(this, MapContext.LocatorsInRegion(Geometry.Region, PlanarPresence))
               from _step in _BurstBack.Capture(this, _loc)
               select _step;

        /// <summary>Provides facility to order burst locators by weakest and closest creatures.</summary>
        public static IEnumerable<Locator> OrderWeakestClosest(IEnumerable<Locator> selection, Point3D nearPoint)
            // capture locators, sorted by power level of chief and distance from near point
            => from _loc in selection
               let _chief = _loc.Chief as Creature
               let _powerLevel = (_chief == null ? 0 : _chief.AdvancementLog.PowerDiceCount)
               orderby _powerLevel, _loc.GeometricRegion.NearDistance(nearPoint)
               select _loc;

        /// <summary>Provides facility to order burst locators by closest creatures.</summary>
        public static IEnumerable<Locator> OrderClosest(IEnumerable<Locator> selection, Point3D nearPoint)
            // capture locators, sorted by power level of chief and distance from near point
            => from _loc in selection
               let _chief = _loc.Chief as Creature
               orderby _loc.GeometricRegion.NearDistance(nearPoint)
               select _loc;
    }
}
