using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class HookedHammer : FragmentGenerator
    {
        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public Striker Striker { get; set; }
        public PickHead PickHead { get; set; }
        public PickHead Tail { get; set; }
        public HaftMount HammerMount { get; set; }
        public HaftMount PickMount { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }
    }
}