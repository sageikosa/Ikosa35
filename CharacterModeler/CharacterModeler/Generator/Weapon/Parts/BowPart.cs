using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class Bow : FragmentPart
    {
        public Bow(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            ThetaDiv = 5;
            CurveSegments = 7;
            LateralTubeThickness = 0.001;
            PolarAxis = new Vector3D(0, 0, 1);
        }

        public bool IsBowString { get; set; }
        public double LateralRadius { get; set; }
        public double PrimaryRadius { get; set; }
        public double LateralTubeThickness { get; set; }
        public double PrimaryTubeThickness { get; set; }
        public int CurveSegments { get; set; }
        public int ThetaDiv { get; set; }
        public int MeshKey { get; set; }
        public string Material { get; set; }
        public Vector3D PolarAxis { get; set; }

        protected XAttribute GetEllipticalTubeAttribute()
        {
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:EllipticalTubeMesh CacheKey={0},PrimaryAngle={1},AngularSpread={2},LateralRadius={3},PrimaryRadius={4},LateralTubeThickness={5},PrimaryTubeThickness={6},CurveSegments={7},ThetaDiv={8},PolarAxis={9} }}",
                MeshKey, (IsBowString ? -1 : 1) * 90d, 180d, LateralRadius, PrimaryRadius, LateralTubeThickness, PrimaryTubeThickness, CurveSegments, ThetaDiv, GetVector3DVal(PolarAxis)));
        }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            return new XElement(winfx + @"GeometryModel3D",
                GetMaterialAttribute(@"Material", Material),
                GetMaterialAttribute(@"BackMaterial", Material),
                GetEllipticalTubeAttribute());
        }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }
    }
}
