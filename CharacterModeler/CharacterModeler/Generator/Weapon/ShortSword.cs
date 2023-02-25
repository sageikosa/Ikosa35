using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class ShortSword : FragmentGenerator
    {
        public ShortSword()
        {
            Grip = new Grip(GetOrigin(), 1);
            Guard = new Guard(Grip.ConnectionPoint(@"Top"));
            Shaft = new BladeShaft(Grip.ConnectionPoint(@"Top"));
            Point = new Point(Shaft.ConnectionPoint(@"Top")) { Bottom = Shaft };
            GripStop = new Stop(Grip.ConnectionPoint(@""), 2);
            this.Grip.Length = 0.4;
            this.Grip.Radius = 0.06;
            this.Guard.Length = 0.075;
            this.Guard.Thickness = 0.175;
            this.Guard.Width = 0.5;
            this.Shaft.Length = 1.1;
            this.Shaft.Thickness = 0.1;
            this.Shaft.Width = 0.15;
            this.Point.Length = 0.2;
            this.GripStop.Radius = 0.08;
            this.GripStop.Length = 0.075;
        }

        public Grip Grip { get; set; }
        public Guard Guard { get; set; }
        public BladeShaft Shaft { get; set; }
        public Point Point { get; set; }
        public Stop GripStop { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, Guard, Shaft, Point, GripStop);
        }
    }
}