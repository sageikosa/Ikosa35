using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class ThrowingAxe : FragmentGenerator
    {
        public ThrowingAxe()
        {
            Haft = new Haft(GetOrigin(),1);
            Blade = new AxeBlade(this.Haft.ConnectionPoint(@"Top"));
        }

        public Haft Haft { get; set; }
        public AxeBlade Blade { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, Haft, Blade);
        }
    }
}
