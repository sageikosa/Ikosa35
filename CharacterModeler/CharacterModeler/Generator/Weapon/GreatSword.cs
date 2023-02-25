using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class GreatSword : FragmentGenerator
    {
        public GreatSword()
        {
            Grip = new Grip(GetOrigin(), 1);
            Guard = new Guard(Grip.ConnectionPoint(@"Top"));
            Shaft = new BladeShaft(Grip.ConnectionPoint(@"Top"));
            Taper = new PointTaper(Shaft.ConnectionPoint(@"Top")) { Bottom = Shaft };
            Point = new Point(Taper.ConnectionPoint(@"Top")) { Bottom = Taper };
            GripStop = new Stop(Grip.ConnectionPoint(@""), 2);
            PommelShaft = new PommelShaft(Grip.ConnectionPoint(@""), 3) { Top = new TopRadiusProvider(Grip, 0.5) };
            PommelBall = new Sphere(PommelShaft.ConnectionPoint(@"Bottom"), 4);

            Grip.Length = 0.75;
            Grip.Radius = 0.08;

            Guard.Length = 0.125;
            Guard.Thickness = 0.25;
            Guard.Width = 0.75;
            Shaft.Length = 2.25;
            Shaft.Thickness = 0.15;
            Shaft.Width = 0.2;
            Taper.Thickness = 0.15;
            Taper.Width = 0.25;
            Taper.Length = 0.25;
            Point.Length = 0.4;
            GripStop.Radius = 0.125;
            GripStop.Length = 0.15;
            PommelShaft.Length = 0.2;
            PommelBall.Radius = 0.125;
        }

        public Grip Grip { get; set; }
        public Guard Guard { get; set; }
        public BladeShaft Shaft { get; set; }
        public PointTaper Taper { get; set; }
        public Point Point { get; set; }
        public Stop GripStop { get; set; }
        public PommelShaft PommelShaft { get; set; }
        public Sphere PommelBall { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, Guard, Shaft, Taper, Point, GripStop, PommelShaft, PommelBall);
        }
    }
}