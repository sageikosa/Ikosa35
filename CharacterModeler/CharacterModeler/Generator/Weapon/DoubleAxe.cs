using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public class DoubleAxe : FragmentGenerator
    {
        public DoubleAxe()
        {
            Pole = new Pole(GetOrigin(), 1)
            {
                Length = 3,
                Radius = 0.0825
            };
            TopBlade = new AxeBlade(Pole.ConnectionPoint(@"Top"))
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
            BottomBlade = new AxeBlade(Pole.ConnectionPoint(@""))
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
            GripSeparation = 0.7d;
            TopGrip = new Grip(() => new Vector3D(0, 0, Pole.Length / 2 + GripSeparation / 2), 2)
            {
                Length = 0.4,
                Radius = 0.1
            };
            BottomGrip = new Grip(() => new Vector3D(0, 0, Pole.Length / 2 - (GripSeparation / 2) - BottomGrip.Length), 3)
            {
                Length = 0.4,
                Radius = 0.1
            };
        }

        public Pole Pole { get; set; }
        public double GripSeparation { get; set; }
        public Grip TopGrip { get; set; }
        public Grip BottomGrip { get; set; }
        public AxeBlade TopBlade { get; set; }
        public AxeBlade BottomBlade { get; set; }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            yield return Pole.GenerateElement(winfx, uzi);
            yield return TopGrip.GenerateElement(winfx, uzi);
            yield return BottomGrip.GenerateElement(winfx, uzi);

            TopBlade.IsFrontward = false;
            yield return TopBlade.GenerateElement(winfx, uzi);
            TopBlade.IsFrontward = true;
            yield return TopBlade.GenerateElement(winfx, uzi);

            BottomBlade.IsFrontward = false;
            yield return BottomBlade.GenerateElement(winfx, uzi);
            BottomBlade.IsFrontward = true;
            yield return BottomBlade.GenerateElement(winfx, uzi);
            yield break;
        }
    }
}
