using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class WarHammer : FragmentGenerator
    {
        public WarHammer()
        {
            Grip = new Grip(GetOrigin(), 1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Mount = new HaftMount(Haft.ConnectionPoint(@"Top"), 3) { IsVertical = false };
            Bus = new HeadBus(Haft.ConnectionPoint(@"Top"), 4);
            Striker1 = new Striker(Bus.ConnectionPoint(@"Left"), 5) { IsLeft = true };
            Striker2 = new Striker(Bus.ConnectionPoint(@"Right"), 6);
            this.Grip.Length = 0.5;
            this.Grip.Radius = 0.1;
            this.Haft.Length = 1;
            this.Haft.Radius = 0.08;
            this.Mount.Length = 0.30;
            this.Mount.Radius = 0.225;
            this.Bus.Length = 0.5;
            this.Bus.Radius = 0.175;
            this.Striker1.Length = 0.30;
            this.Striker1.Radius = 0.225;
            this.Striker2.Length = 0.30;
            this.Striker2.Radius = 0.225;
        }

        public Haft Haft { get; set; }
        public HeadBus Bus { get; set; }
        public Grip Grip { get; set; }
        public Striker Striker1 { get; set; }
        public Striker Striker2 { get; set; }
        public HaftMount Mount { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, Haft, Mount, Bus, Striker1, Striker2);
        }
    }
}