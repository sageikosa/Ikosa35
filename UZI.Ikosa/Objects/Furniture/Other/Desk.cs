using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Desk : Furnishing
    {
        public Desk(Items.Materials.Material material)
            : base(nameof(Desk), material)
        {
        }

        public override bool IsHardSnap(AnchorFace face)
            => (Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Left) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Right) == face);

        protected override AnchorFaceList GetCoveredFaces()
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top).ToAnchorFaceList()
            .Add(Orientation.GetAnchorFaceForSideIndex(SideIndex.Left))
            .Add(Orientation.GetAnchorFaceForSideIndex(SideIndex.Right));

        public override bool IsUprightAllowed => true;

        public override object Clone()
            => new Desk(ObjectMaterial);

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        protected override string ClassIconKey
            => nameof(Desk);
    }
}
