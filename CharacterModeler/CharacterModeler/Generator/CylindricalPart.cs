using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public abstract class CylindricalPart : FragmentPart
    {
        protected CylindricalPart(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            ThetaDiv = 7;
        }

        public double Length { get; set; }
        public double Radius { get; set; }
        public int ThetaDiv { get; set; }
        public int MeshKey { get; set; }
        public double Rotation { get; set; }

        protected abstract string MaterialKey { get; }
        protected abstract Point3D StartPoint();
        protected abstract Point3D EndPoint();

        protected XAttribute GetCylinderAttribute()
        {
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:CylinderMesh CacheKey={0},Diameter={1},ThetaDiv={2},BaseCap={3},TopCap={4},P1={5},P2={6},Rotation={7} }}",
                MeshKey, Radius * 2, ThetaDiv, true, true, GetPoint3DVal(StartPoint()), GetPoint3DVal(EndPoint()), Rotation));
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
