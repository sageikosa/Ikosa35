using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class TwoBladedSword : FragmentGenerator
    {
        public TwoBladedSword()
        {
            Grip = new Grip(GetOrigin(), 1);
            TopShaft = new BladeShaft(Grip.ConnectionPoint(@"Top"));
            TopPoint = new Point(TopShaft.ConnectionPoint(@"Top")) { Bottom = TopShaft };
            BottomShaft = new BladeShaft(Grip.ConnectionPoint(@"Top")) { IsInverted = true };
            BottomPoint = new Point(BottomShaft.ConnectionPoint(@"Top")) { Bottom = TopShaft, IsInverted = true };
        }

        public BladeShaft TopShaft { get; set; }
        public BladeShaft BottomShaft { get; set; }
        public Point TopPoint { get; set; }
        public Point BottomPoint { get; set; }
        public Grip Grip { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Grip, TopShaft, TopPoint, BottomShaft, BottomPoint);
        }
    }
}