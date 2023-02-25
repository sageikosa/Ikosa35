using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class WarAxe : FragmentGenerator
    {
        public WarAxe()
        {
            Grip = new Grip(GetOrigin(), 1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Blade = new AxeBlade(Haft.ConnectionPoint(@"Top"));
            Back = new WaraxeBack(Haft.ConnectionPoint(@"Top"));
        }

        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public AxeBlade Blade { get; set; }
        public WaraxeBack Back { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Grip.GenerateElement(winfx, uzi);
            yield return Haft.GenerateElement(winfx, uzi);
            yield return Blade.GenerateElement(winfx, uzi);
            yield return Back.GenerateElement(winfx, uzi);
            yield break;
        }
    }
}