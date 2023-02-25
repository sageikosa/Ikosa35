using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class HeadBus : CylindricalPart
    {
        public HeadBus(Func<Vector3D> origin, int meshKey) : base(origin, meshKey) { }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Left":
                    return () => Origin() + new Vector3D(0 - (Length / 2), 0, 0);
                case @"Right":
                    return () => Origin() + new Vector3D(Length / 2, 0, 0);
            }
            return base.ConnectionPoint(key);
        }

        protected override string MaterialKey { get { return @"HeadBus"; } }

        protected override Point3D StartPoint()
        {
            return MyPoint() + new Vector3D(0 - Length / 2, 0, 0);
        }

        protected override Point3D EndPoint()
        {
            return MyPoint() + new Vector3D(Length / 2, 0, 0);
        }
    }
}
