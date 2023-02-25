using System;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class CaptureZone : ISourcedObject
    {
        #region ctor(...)
        protected CaptureZone(MapContext mapContext, object source, Geometry geometry, PlanarPresence planar)
            : this(mapContext, source, geometry, planar, new Intersection(0, 0, 0))
        {
        }

        protected CaptureZone(MapContext mapContext, object source, Geometry geometry, PlanarPresence planar,
            IGeometryAnchorSupplier origin)
        {
            _MapContext = mapContext;
            _Source = source;
            _Geometry = geometry;
            _Origin = origin;
            _LineToOrigin = true;
            _Planar = planar;
        }
        #endregion

        #region state
        private MapContext _MapContext;
        private IGeometryAnchorSupplier _Origin;
        private bool _LineToOrigin;
        protected Geometry _Geometry;
        private object _Source;
        private PlanarPresence _Planar;
        #endregion

        public MapContext MapContext => _MapContext;
        public IGeometryAnchorSupplier Origin => _Origin;
        public bool LineToOrigin => _LineToOrigin;
        public virtual object Source => _Source;
        public PlanarPresence PlanarPresence => _Planar;

        /// <summary>Specifies the geometric shape of the interaction zone: Cone, Sphere, Cube, etc.</summary>
        public Geometry Geometry => _Geometry;

        /// <summary>Unhooks geometry and calls OnRemoveZone() abstraction</summary>
        public void RemoveZone()
        {
            Geometry?.Supplier?.RemoveChangeMonitor(Geometry);
            OnRemoveZone();
        }

        protected abstract void OnRemoveZone();

        /// <summary>
        /// Calls the cell-based IsCellInGeometry for each cell of the geomExtent to determine if the geomExtent is in the geometry
        /// </summary>
        /// <param name="geomExtent">geometric extent to test</param>
        /// <returns>true if some cell of the geomExtent is in the geometry</returns>
        /// <remarks>geomExtent is tested by iterating inclusion.  It is not a cell-by-cell intersection.</remarks>
        public bool ContainsGeometricRegion(IGeometricRegion geomExtent, ICoreObject targetObject, PlanarPresence planar)
            => geomExtent?.AllCellLocations().Any(_cl => ContainsCell(_cl, targetObject, planar)) ?? false;

        /// <summary>
        /// Calls coordinate-based ContainsCell to determine exact inclusion (with line to origin if necessary)
        /// </summary>
        /// <param name="cellLocation">cell location to test</param>
        /// <returns>true if cell location falls in the geometry</returns>
        public bool ContainsCell(ICellLocation cellLocation, ICoreObject targetObject, PlanarPresence planar)
            => (cellLocation != null)
            && PlanarPresence.HasOverlappingPresence(planar)
            && _Geometry.Region.ContainsCell(cellLocation)
            && OnIsCellInGeometry(cellLocation, targetObject);

        protected virtual bool OnIsCellInGeometry(ICellLocation location, ICoreObject targetObject)
            => !LineToOrigin
            || MapContext.Map.HasLineOfEffect(Origin.GetPoint3D(), new CellPosition(Geometry.Supplier.Location), location, PlanarPresence);
    }
}