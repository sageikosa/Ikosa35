using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public abstract class ConicPart : FragmentPart, IBottomRadiusProvider, ITopRadiusProvider
    {
        protected ConicPart(Func<Vector3D> origin, int meshKey)
            : base(origin)
        {
            MeshKey = meshKey;
            ThetaDiv = 7;
        }

        public int ThetaDiv { get; set; }
        public int MeshKey { get; set; }
        public bool BaseCap { get; set; }
        public bool TopCap { get; set; }
        public Vector3D Direction { get; set; }
        public double Length { get; set; }
        public bool FlatMap { get; set; }

        // TODO: origin & destination scale

        protected abstract string BackMaterialKey { get; }
        protected abstract string MaterialKey { get; }
        protected abstract double BaseRadius();
        protected abstract double TopRadius();
        protected abstract Vector3D Skew();

        protected XAttribute GetConeAttribute()
        {
            // TODO: origin & destination scale
            return new XAttribute(@"Geometry",
                string.Format(@"{{uzi:ConeMesh CacheKey={0},Origin={1},Direction={2},BaseRadius={3},TopRadius={4},Height={5},ThetaDiv={6},BaseCap={7},TopCap={8},Skew={9},FlatMap={10} }}",
                MeshKey, GetPoint3DVal(MyPoint()), GetVector3DVal(Direction), BaseRadius(), TopRadius(), Length,
                ThetaDiv, BaseCap, TopCap, GetVector3DVal(Skew()), FlatMap));
        }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            if (!String.IsNullOrEmpty(BackMaterialKey))
                return new XElement(winfx + @"GeometryModel3D",
                    GetMaterialAttribute(@"Material", MaterialKey),
                    GetMaterialAttribute(@"BackMaterial", BackMaterialKey),
                    GetConeAttribute());
            else
                return new XElement(winfx + @"GeometryModel3D",
                    GetMaterialAttribute(@"Material", MaterialKey),
                    GetConeAttribute());
        }

        #region ITopRadiusProvider Members

        double ITopRadiusProvider.Radius
        {
            get { return BaseRadius(); }
        }

        double ITopDimensionProvider.Thickness
        {
            get { return BaseRadius() * 2; }
        }

        double ITopDimensionProvider.Width
        {
            get { return BaseRadius() * 2; }
        }

        #endregion

        #region IBottomRadiusProvider Members

        double IBottomRadiusProvider.Radius
        {
            get { return TopRadius(); }
        }

        double IBottomDimensionProvider.Thickness
        {
            get { return TopRadius() * 2; }
        }

        double IBottomDimensionProvider.Width
        {
            get { return TopRadius() * 2; }
        }

        #endregion
    }
}