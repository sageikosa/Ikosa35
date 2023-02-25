using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class HeavyMace : FragmentGenerator
    {
        public HeavyMace()
        {
            Grip = new Grip(GetOrigin(), 1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Head = new MaceHead(Haft.ConnectionPoint(@"Top"), true, 3) { Bottom = Haft, TopCap = false };
            OverHead = new MaceOverhead(Head.ConnectionPoint(@"Top"), 4) { Top = Haft, BaseCap = true, TopCap = true };
            this.Grip.Length = 0.5;
            this.Grip.Radius = 0.1;
            this.Haft.Length = 0.5;
            this.Haft.Radius = 0.08;
            this.Head.Radius = 0.2;
            this.Head.Length = 0.3;
            this.OverHead.Radius = 0.3;
            this.OverHead.Length = 0.35;
        }

        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public MaceHead Head { get; set; }
        public MaceOverhead OverHead { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Grip.GenerateElement(winfx, uzi);
            yield return Haft.GenerateElement(winfx, uzi);
            yield return Head.GenerateElement(winfx, uzi);
            yield return OverHead.GenerateElement(winfx, uzi);
            yield break;
        }
    }
}