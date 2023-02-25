using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Dagger : FragmentGenerator
    {
        public Dagger()
        {
            Grip = new Grip(GetOrigin(),1);
            Guard = new Guard(Grip.ConnectionPoint(@"Top"));
            Shaft = new BladeShaft(Grip.ConnectionPoint(@"Top"));
            Point = new Point(Shaft.ConnectionPoint(@"Top")) { Bottom = Shaft };
            Pommel = new SmallPommel(Grip.ConnectionPoint(@""), 2) { Top = Grip };
            this.Grip.Length = 0.33;
            this.Grip.Radius = 0.045;
            this.Guard.Length = 0.05;
            this.Guard.Thickness = 0.175;
            this.Guard.Width = 0.333;
            this.Shaft.Length = 0.66;
            this.Shaft.Thickness = 0.075;
            this.Shaft.Width = 0.1;
            this.Point.Length = 0.15;
        }

        public Grip Grip { get; set; }
        public Guard Guard { get; set; }
        public BladeShaft Shaft { get; set; }
        public Point Point { get; set; }
        public SmallPommel Pommel { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, Guard, Shaft, Point, Pommel);
        }

    }
}
