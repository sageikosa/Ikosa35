using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Stop : CylindricalPart, ITopRadiusProvider
    {
        public Stop(Func<Vector3D> origin, int meshKey) : base(origin, meshKey) { }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string MaterialKey { get { return @"Stop"; } }

        protected override Point3D StartPoint()
        {
            return MyPoint();
        }

        protected override Point3D EndPoint()
        {
            return MyPoint() + new Vector3D(0, 0, Length);
        }

        #region ITopDimensionProvider Members

        public double Thickness
        {
            get { return Radius / 2; }
        }

        public double Width
        {
            get { return Radius / 2; }
        }

        #endregion
    }
}
