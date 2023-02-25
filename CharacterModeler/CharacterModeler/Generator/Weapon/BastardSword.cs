using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class BastardSword : FragmentGenerator
    {
        public BastardSword()
        {
            Grip = new Grip(GetOrigin(), 1);
            Guard = new Guard(Grip.ConnectionPoint(@"Top"));
            Shaft = new BladeShaft(Grip.ConnectionPoint(@"Top"));
            Point = new Point(Shaft.ConnectionPoint(@"Top")) { Bottom = Shaft };
            GripStop = new Stop(Grip.ConnectionPoint(@""), 2);
            PommelShaft = new PommelShaft(Grip.ConnectionPoint(@""), 3) { Top = new TopRadiusProvider(Grip, 0.5) };
            PommelBall = new Sphere(PommelShaft.ConnectionPoint(@"Bottom"), 4);

            Grip.Length = 0.625;
            Grip.Radius = 0.075;

            Guard.Length = 0.1;
            Guard.Thickness = 0.225;
            Guard.Width = 0.75;
            Shaft.Length = 2;
            Shaft.Thickness = 0.125;
            Shaft.Width = 0.175;
            Point.Length = 0.3;
            GripStop.Radius = 0.1125;
            GripStop.Length = 0.125;
            PommelShaft.Length = 0.175;
            PommelBall.Radius = 0.1;
        }

        public Grip Grip { get; set; }
        public Guard Guard { get; set; }
        public BladeShaft Shaft { get; set; }
        public Point Point { get; set; }
        public Stop GripStop { get; set; }
        public PommelShaft PommelShaft { get; set; }
        public Sphere PommelBall { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, Guard, Shaft,  Point, GripStop, PommelShaft, PommelBall);
        }
    }
}
