using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class MaceOverhead : ConicPart
    {
        public MaceOverhead(Func<Vector3D> origin, int meshKey)
            : base(origin, meshKey)
        {
            Direction = new Vector3D(0, 0, 1);
            ThetaDiv = 5;
        }

        public double Height { get; set; }
        public double Radius { get; set; }
        public ITopRadiusProvider Top { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string BackMaterialKey { get { return string.Empty; } }
        protected override string MaterialKey { get { return @"MaceOverhead"; } }
        protected override double BaseRadius() { return Radius; }
        protected override double TopRadius() { return Top.Radius; }
        protected override Vector3D Skew() { return new Vector3D(); }
    }
}
