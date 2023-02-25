using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Shield : FragmentGenerator
    {
        public Shield()
        {
            Rim = new ShieldRim(GetOrigin(), 1)
            {
                Length = 0.25,
                InnerRadius = 0.75,
                OuterRadius = 0.9,
                Direction = new System.Windows.Media.Media3D.Vector3D(0, 0, 1)
            };
            Face = new ShieldFace(Rim.ConnectionPoint(@"Center"), 2)
            {
                Bottom = Rim,
                Length = 0.25,
                FlatMap = true,
                Direction = new System.Windows.Media.Media3D.Vector3D(0, 0, 1)
            };
        }

        public ShieldRim Rim { get; set; }
        public ShieldFace Face { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Rim, Face);
        }
    }
}