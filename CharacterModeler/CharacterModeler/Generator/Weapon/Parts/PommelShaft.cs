using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class PommelShaft : FragmentPart
    {
        public PommelShaft(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            ThetaDiv = 7;
        }

        public ITopRadiusProvider Top { get; set; }
        public int MeshKey { get; set; }
        public int ThetaDiv { get; set; }
        public double Rotation { get; set; }
        public double Length { get; set; }

        public string MaterialKey { get { return @"PommelShaft"; } }

        protected Point3D StartPoint()
        {
            return MyPoint() - new Vector3D(0, 0, Length);
        }

        protected Point3D EndPoint()
        {
            return MyPoint();
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Top":
                    return  base.ConnectionPoint(key);
            }
            return () => Origin() - new Vector3D(0, 0, Length);
        }

        protected XAttribute GetCylinderAttribute()
        {
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:CylinderMesh CacheKey={0},Diameter={1},ThetaDiv={2},BaseCap={3},TopCap={4},P1={5},P2={6},Rotation={7} }}",
                MeshKey, Top.Radius * 2, ThetaDiv, false, false, GetPoint3DVal(StartPoint()), GetPoint3DVal(EndPoint()), Rotation));
        }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return new XElement(winfx + @"GeometryModel3D",
                GetMaterialAttribute(@"Material", MaterialKey),
                GetCylinderAttribute());
        }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }
    }
}
