using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Javelin : FragmentGenerator
    {
        public Javelin()
        {
            Pole = new Pole(GetOrigin(), 1);
            Taper = new PointTaper(Pole.ConnectionPoint(@"Top")) { Bottom = Pole };
            Point = new Point(Taper.ConnectionPoint(@"Top")) { Bottom = Taper };

            Pole.Length = 4;
            Pole.Radius = 0.0725;
            Taper.Length = 0.2;
            Taper.Thickness = 0.3;
            Taper.Width = 0.2;
            Point.Length = 0.35;
        }

        public Pole Pole { get; set; }
        public Point Point { get; set; }
        public PointTaper Taper { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Pole, Taper, Point);
        }
    }
}