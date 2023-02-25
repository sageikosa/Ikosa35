using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CharacterModeler.Generator
{
    public abstract class Crossbow : FragmentGenerator
    {
        protected Crossbow()
        {
            BowCenterThickness = 0.1d;
            BowEndThickness = 0.025d;
            StringThickness = 0.01d;
        }

        public double BowCenterThickness { get; set; }
        public double BowEndThickness { get; set; }
        public double StringThickness { get; set; }

        public double BowLength { get; set; }
        public double BowWidth { get; set; }
        public double DrawBackFactor { get; set; }
        public double BarrelLength { get; set; }
        public double BarrelWidth { get; set; }

        private Bow GetBow()
        {
            return new Bow(GetOrigin(), 1)
            {
                Material = @"Bow",
                LateralRadius = BowLength / 2,
                PrimaryRadius = BowWidth,
                PrimaryTubeThickness = BowCenterThickness,
                LateralTubeThickness = BowEndThickness,
                PolarAxis = new Vector3D(0, 0, 1)
            };
        }

        private Bow GetBowString()
        {
            return new Bow(GetOrigin(), 2)
            {
                Material = @"BowString",
                LateralRadius = BowLength / 2,
                PrimaryRadius = DrawBackFactor * BowWidth,
                PrimaryTubeThickness = StringThickness,
                LateralTubeThickness = StringThickness,
                CurveSegments = 2,
                PolarAxis = new Vector3D(0, 0, 1),
                IsBowString = true
            };
        }

        protected CrossbowBarrel GetBarrel()
        {
            return new CrossbowBarrel(GetOffset(GetOrigin(), () => new Vector3D(0, BowWidth, 0)), 3)
            {
                Length = BarrelLength,
                Radius = BarrelWidth / 2,
                ThetaDiv = 5,
                Rotation = 45
            };
        }

        protected override IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi)
        {
            return WriteFromFlatList(winfx, uzi, GetBow(), GetBowString(), GetBarrel());
        }
    }
}
