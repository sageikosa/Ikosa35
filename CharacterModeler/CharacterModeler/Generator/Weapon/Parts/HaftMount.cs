using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class HaftMount : CylindricalPart
    {
        public HaftMount(Func<Vector3D> origin, int meshKey) : base(origin, meshKey) { }

        public bool IsVertical { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string MaterialKey { get { return @"HaftMount"; } }

        protected override Point3D StartPoint()
        {
            if (IsVertical)
                return MyPoint();
            else
                return MyPoint() + new Vector3D(0 - Length / 2, 0, 0);
        }

        protected override Point3D EndPoint()
        {
            if (IsVertical)
                return MyPoint() + new Vector3D(0, 0, Length);
            else
                return MyPoint() + new Vector3D(Length / 2, 0, 0);
        }
    }
}
