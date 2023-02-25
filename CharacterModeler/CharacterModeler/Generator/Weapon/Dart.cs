using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Dart : FragmentGenerator
    {
        public Dart()
        {
            Tail = new DartTail(GetOrigin(), 1);
            Pole = new Pole(Tail.ConnectionPoint(@"Top"), 2);
            Taper = new PointTaper(Pole.ConnectionPoint(@"Top")) { Bottom = Pole };
            Point = new Point(Taper.ConnectionPoint(@"Top")) { Bottom = Taper };

            Tail.Length = 0.33;
            Tail.Top = Pole;
            Tail.Radius = 0.125;
            Pole.Length = 1.5;
            Pole.Radius = 0.05;
            Taper.Length = 0.15;
            Taper.Thickness = 0.2;
            Taper.Width = 0.15;
            Point.Length = 0.3;
        }

        public Pole Pole { get; set; }
        public Point Point { get; set; }
        public PointTaper Taper { get; set; }
        public DartTail Tail { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Pole, Taper, Point, Tail);
        }
    }
}
