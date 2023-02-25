using System;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SweepPivotPortal : PortalBase
    {
        public SweepPivotPortal(string name, PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, portObjA, portObjB)
        {
        }

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        public override Sizer Sizer => null;

        protected override bool IsSideAccessible(bool inside, IGeometricRegion actor, IGeometricRegion portal)
        {
            return true;
        }
    }
}
