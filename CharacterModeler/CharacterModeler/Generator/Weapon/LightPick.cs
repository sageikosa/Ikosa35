using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class LightPick : FragmentGenerator
    {
        public Haft Haft { get; set; }
        public PickHead Head { get; set; }
        public PickTail Tail { get; set; }
        public Stop Stop { get; set; }
        public HaftMount Mount { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }
    }
}
