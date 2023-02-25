using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class ShieldRim : ConicPart
    {
        public ShieldRim(Func<Vector3D> origin, int meshKey)
            : base(origin, meshKey)
        {
            ThetaDiv = 9;
        }

        public double InnerRadius { get; set; }
        public double OuterRadius { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Center":
                    return () => Origin() + new Vector3D(0, 0, Length);
            }
            return base.ConnectionPoint(key);
        }

        protected override string MaterialKey { get { return @"ShieldRim"; } }
        protected override double BaseRadius() { return OuterRadius; }
        protected override double TopRadius() { return InnerRadius; }
        protected override Vector3D Skew() { return new Vector3D(); }
        protected override string BackMaterialKey { get { return @"ShieldRimBack"; } }
    }
}