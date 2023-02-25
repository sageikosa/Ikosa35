using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class CrossbowBarrel : CylindricalPart
    {
        public CrossbowBarrel(Func<Vector3D> origin, int meshKey)
            : base(origin, meshKey)
        {
        }

        protected override string MaterialKey
        {
            get { return @"Barrel"; }
        }

        protected override Point3D StartPoint()
        {
            return this.MyPoint();
        }

        protected override Point3D EndPoint()
        {
            return MyPoint() + new Vector3D(0, -Length, 0);
        }
    }
}
