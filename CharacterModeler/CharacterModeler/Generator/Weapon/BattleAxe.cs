using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class BattleAxe : FragmentGenerator
    {
        public BattleAxe()
        {
            Grip = new Grip(GetOrigin(), 1);
            Haft = new Haft(Grip.ConnectionPoint(@"Top"), 2);
            Blade = new AxeBlade(Haft.ConnectionPoint(@"Top"))
            {
                BladeHeight = 0.33,
                BladeLength = 0.25,
                Length = 0.625,
                TopHeight = 0.25,
                BottomHeight = 0.25,
                TopBackLength = 0.1,
                BottomBackLength = 0.1,
                Thickness = 0.175
            };
            HaftMount = new HaftMount(Blade.ConnectionPoint(@"Bottom"), 3)
            {
                IsVertical = true,
                Length = 0.33,
                Radius = 0.1
            };

            // dimensions
            this.Grip.Length = 0.5;
            this.Grip.Radius = 0.08;
            this.Haft.Length = 0.9;
            this.Haft.Radius = 0.06;
        }

        public Haft Haft { get; set; }
        public Grip Grip { get; set; }
        public AxeBlade Blade { get; set; }
        public HaftMount HaftMount { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Grip.GenerateElement(winfx, uzi);
            yield return Haft.GenerateElement(winfx, uzi);
            yield return Blade.GenerateElement(winfx, uzi);
            yield return HaftMount.GenerateElement(winfx, uzi);
            yield break;
        }
    }
}