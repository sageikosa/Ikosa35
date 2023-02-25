using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Spike : ConicPart
    {
        public Spike(Func<Vector3D> origin, int meshKey) : base(origin, meshKey) { }

        public double Radius { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string MaterialKey { get { return @"spike"; } }
        protected override double BaseRadius() { return Radius; }
        protected override double TopRadius() { return 0; }
        protected override Vector3D Skew() { return new Vector3D(); }
        protected override string BackMaterialKey { get { return string.Empty; } }
    }
}
