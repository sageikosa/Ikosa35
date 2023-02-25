using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Halberd : FragmentGenerator
    {
        public Pole Pole { get; set; }
        public Point Point { get; set; }
        public AxeBlade Head { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            throw new NotImplementedException();
        }
    }
}
