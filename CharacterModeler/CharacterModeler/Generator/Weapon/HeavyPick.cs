using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class HeavyPick : FragmentGenerator
    {
        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public PickHead Head { get; set; }
        public Point Point { get; set; }
        public HaftMount Mount { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }
    }
}
