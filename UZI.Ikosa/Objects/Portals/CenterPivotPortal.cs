using System;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class CenterPivotPortal : PortalBase
    {
        public CenterPivotPortal(string name, PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, portObjA, portObjB)
        {
        }
        // NOTE: must be in a square footed locator
        //       ... if even, then pivot is within a cell
        //       ... if odd, then pivot is at an intersection
        // NOTE: the center pivot portal has 8 positions

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        public override Sizer Sizer => null;

        protected override bool IsSideAccessible(bool inside, IGeometricRegion actor, IGeometricRegion portal)
        {
            return true;
        }
    }
}
