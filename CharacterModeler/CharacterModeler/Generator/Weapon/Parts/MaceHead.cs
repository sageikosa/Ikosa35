using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class MaceHead : ConicPart
    {
        public MaceHead(Func<Vector3D> origin, bool isFlush, int meshKey)
            : base(origin, meshKey)
        {
            if (!isFlush)
                Origin = FragmentPart.GetOffset(origin, () => new Vector3D(0, 0, 0 - Length * 1.1));
            Direction = new Vector3D(0, 0, 1);
            ThetaDiv = 5;
        }

        public IBottomRadiusProvider Bottom { get; set; }
        public double Radius { get; set; }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Top":
                    return () => Origin() + new Vector3D(0, 0, Length);
            }
            return base.ConnectionPoint(key);
        }

        protected override string BackMaterialKey { get { return string.Empty; } }
        protected override string MaterialKey { get { return @"MaceHead"; } }
        protected override double BaseRadius() { return Bottom.Radius; }
        protected override double TopRadius() { return Radius; }
        protected override Vector3D Skew() { return new Vector3D(); }
    }
}