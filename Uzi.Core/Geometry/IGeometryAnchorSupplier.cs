using System;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Core
{
    public interface IGeometryAnchorSupplier : IControlChange<ICellLocation>
    {
        LocationAimMode LocationAimMode { get; }
        ICellLocation Location { get; }
    }

    public static class IGeometryAnchorSupplierHelper
    {
        public static Point3D GetPoint3D(this IGeometryAnchorSupplier self)
            => self.LocationAimMode.GetPoint3D(self.Location);
    }
}
