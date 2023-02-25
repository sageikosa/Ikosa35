using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class SmallPommel : FragmentPart
    {
        public SmallPommel(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            ThetaDiv = 6;
            PhiDiv = 6;
        }

        public ITopRadiusProvider Top { get; set; }
        public int MeshKey { get; set; }
        public int ThetaDiv { get; set; }
        public int PhiDiv { get; set; }

        protected XAttribute GetSphereAttribute()
        {
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:SphereMesh CacheKey={0},Radius={1},ThetaDiv={2},PhiDiv={3},Center={4} }}",
                MeshKey, Top.Radius * 0.85, ThetaDiv, PhiDiv, GetPoint3DVal(MyPoint())));
        }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return new XElement(winfx + @"GeometryModel3D",
                GetMaterialAttribute(@"Material", @"Pommel"),
                GetSphereAttribute());
        }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }
    }
}
