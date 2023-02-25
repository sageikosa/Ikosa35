using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Sphere : FragmentPart
    {
        public Sphere(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            Radius = 1d;
            ThetaDiv = 20;
            PhiDiv = 10;
            MaterialKey = @"Sphere";
        }

        public double Radius { get; set; }
        public int ThetaDiv { get; set; }
        public int PhiDiv { get; set; }
        public int MeshKey { get; set; }
        public string MaterialKey { get; set; }

        protected XAttribute GetSphereAttribute()
        {
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:SphereMesh CacheKey={0},Radius={1},ThetaDiv={2},PhiDiv={3},Center={4} }}",
                MeshKey, Radius, ThetaDiv, PhiDiv, GetPoint3DVal(MyPoint())));
        }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return new XElement(winfx + @"GeometryModel3D",
                GetMaterialAttribute(@"Material", MaterialKey),
                GetSphereAttribute());
        }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }
    }
}
