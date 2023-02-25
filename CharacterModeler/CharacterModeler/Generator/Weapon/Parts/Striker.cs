using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Striker : CylindricalPart
    {
        public Striker(Func<Vector3D> origin, int meshKey) : base(origin, meshKey) { }

        public bool IsLeft { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string MaterialKey { get { return @"Striker"; } }

        protected override Point3D StartPoint()
        {
            return MyPoint();
        }

        protected override Point3D EndPoint()
        {
            if (IsLeft)
                return MyPoint() - new Vector3D(Length, 0, 0);
            return MyPoint() + new Vector3D(Length, 0, 0);
        }
    }
}
