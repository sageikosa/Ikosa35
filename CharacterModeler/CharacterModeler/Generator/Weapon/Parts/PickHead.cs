using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class PickHead : FragmentPart
    {
        public PickHead(Func<Vector3D> origin) : base(origin) { }

        public double Length { get; set; }
        public double CurveOffset { get; set; }
        public double Height { get; set; }
        public double Thickness { get; set; }

        public override XElement GenerateElement(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }
    }
}
