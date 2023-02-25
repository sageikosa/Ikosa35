using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Barrel : HollowFurnishing
    {
        public Barrel(Material material)
            : base(nameof(Barrel), material)
        {
        }

        public override HollowFurnishingLid CreateLid(Material material)
            => new BarrelLid(material)
            {
                Width = Width,
                Length = Length,
                Height = 0.0825,
                TareWeight = TareWeight / 2,
                MaxStructurePoints = MaxStructurePoints / 2
            };

        public override bool IsHardSnap(AnchorFace face)
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top).GetAxis() == face.GetAxis();

        protected override HollowFurnishing GetClone()
            => new Barrel(ObjectMaterial);

        protected override string ClassIconKey
            => nameof(Barrel);
    }

    [Serializable]
    public class BarrelLid : HollowFurnishingLid
    {
        public BarrelLid(Material material)
            : base(@"Barrel Lid", material)
        {
        }

        public override object Clone()
            => new BarrelLid(ObjectMaterial);

        public override bool IsValidLid(HollowFurnishing hollowFurnishing)
            => hollowFurnishing is Barrel;

        protected override string ClassIconKey => nameof(BarrelLid);
    }
}
