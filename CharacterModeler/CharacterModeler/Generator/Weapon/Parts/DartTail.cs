using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class DartTail : ConicPart
    {
        public DartTail(Func<Vector3D> origin, int meshKey)
            : base(origin, meshKey)
        {
            Direction = new Vector3D(0, 0, 1);
            ThetaDiv = 5;
            BaseCap = true;
        }

        public double Radius { get; set; }
        public ITopRadiusProvider Top { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        protected override string BackMaterialKey { get { return string.Empty; } }
        protected override string MaterialKey { get { return @"DartTail"; } }
        protected override double BaseRadius() { return Radius; }
        protected override double TopRadius() { return Top.Radius; }
        protected override Vector3D Skew() { return new Vector3D(); }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            if (key == @"Top")
            {
                return () => Origin() + new Vector3D(0, 0, Length);
            }
            return base.ConnectionPoint(key);
        }
    }
}
