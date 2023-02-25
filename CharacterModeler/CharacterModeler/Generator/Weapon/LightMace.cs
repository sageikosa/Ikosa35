using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class LightMace : FragmentGenerator
    {
        public LightMace()
        {
            Grip = new Grip(GetOrigin(), 1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Head = new MaceHead(Haft.ConnectionPoint(@"Top"), false, 3) { Bottom = Haft, TopCap = true };
            this.Grip.Length = 0.5;
            this.Grip.Radius = 0.08;
            this.Haft.Length = 0.9;
            this.Haft.Radius = 0.06;
            this.Head.Length = 0.3;
            this.Head.Radius = 0.2;
        }

        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public MaceHead Head { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Grip.GenerateElement(winfx, uzi);
            yield return Haft.GenerateElement(winfx, uzi);
            yield return Head.GenerateElement(winfx, uzi);
            yield break;
        }
    }
}