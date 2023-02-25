using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Longspear : FragmentGenerator
    {
        public Longspear()
        {
            Pole = new Pole(GetOrigin(), 1);
            Taper = new PointTaper(Pole.ConnectionPoint(@"Top")) { Bottom = Pole };
            Point = new Point(Taper.ConnectionPoint(@"Top")) { Bottom = Taper };
        }

        public Pole Pole { get; set; }
        public Point Point { get; set; }
        public PointTaper Taper { get; set; }
        public Stop EndStop { get; set; }
        public Stop GripStop { get; set; }
        public Stop TopStop { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }
    }
}
