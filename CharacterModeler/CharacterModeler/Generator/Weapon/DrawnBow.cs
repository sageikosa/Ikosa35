using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public abstract class DrawnBow : FragmentGenerator
    {
        protected DrawnBow()
        {
            GripThickness = 0.1d;
            EndThickness = 0.025d;
            StringThickness = 0.01d;
        }

        public double Length { get; set; }
        public double Width { get; set; }
        public double GripThickness { get; set; }
        public double EndThickness { get; set; }
        public double StringThickness { get; set; }
        public double DrawBackFactor { get; set; }

        private Bow GetBow()
        {
            return new Bow(GetOrigin(), 1)
            {
                Material = @"Bow",
                LateralRadius = Length / 2,
                PrimaryRadius = Width,
                PrimaryTubeThickness = GripThickness,
                LateralTubeThickness = EndThickness,
                PolarAxis = new Vector3D(1, 0, 0)
            };
        }

        private Bow GetBowString()
        {
            return new Bow(GetOrigin(), 2)
            {
                Material = @"BowString",
                LateralRadius = Length / 2,
                PrimaryRadius = DrawBackFactor * Width,
                PrimaryTubeThickness = StringThickness,
                LateralTubeThickness = StringThickness,
                CurveSegments = 2,
                PolarAxis = new Vector3D(1, 0, 0),
                IsBowString = true
            };
        }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, GetBow(), GetBowString());
        }
    }
}