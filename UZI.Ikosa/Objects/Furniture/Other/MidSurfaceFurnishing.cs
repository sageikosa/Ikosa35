using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class MidSurfaceFurnishing : Furnishing
    {
        protected MidSurfaceFurnishing(string name, Items.Materials.Material material)
            : base(name, material)
        {
        }

        protected abstract MidSurfaceFurnishing GetClone();

        public override bool IsUprightAllowed => true;

        public override object Clone()
        {
            var _clone = GetClone();
            _clone.CopyFrom(this);
            return _clone;
        }

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        // TODO: if surface is actor would be "cut" by surface, then surface blocks
        // TODO: consider low surface (step onto), high surface (crawl under/hunch over)
        public override bool BlocksTransit(MovementTacticalInfo moveTactical)
        {
            if (!moveTactical.Movement.CanMoveThrough(ObjectMaterial))
            {
                // if base blocks, then blocking
                if (base.BlocksTransit(moveTactical))
                {
                    return true;
                }

                // surface orthogonal axis
                var _axis = Orientation.GetAnchorFaceForSideIndex(SideIndex.Top).GetAxis();
                if (moveTactical.TransitFaces.Any(_tf => _tf.GetAxis() == _axis))
                {
                    // if moving along same axis as surface ortho orientation, no need to check block
                    return false;
                }

                // TODO: if actor would be "cut" by surface, then surface blocks
                // TODO: consider low surface (step onto), high surface (crawl under/hunch over)
                return !moveTactical.Movement.CanMoveThrough(ObjectMaterial)
                    && Orientation.GetPlanarPoints(AnchorFaceList.All)
                    .Any(_f => _f.SegmentIntersection(moveTactical.SourceCell.GetPoint(),
                        moveTactical.TargetCell.GetPoint()).HasValue);
            }
            return false;
        }

        #region public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        {
            if (!base.CanOccupy(movement, region, testObj))
            {
                // TODO: if base allows it we're probably OK
                return false;
            }
            return true;
        }
        #endregion

        #region public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        /// <summary>Used to determine support against gravity and starting elevations</summary>
        public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        {
            // TODO: influenced!
            if (!moveTactical.Movement.CanMoveThrough(ObjectMaterial))
            {
                // reverse transit faces must be snapped (should only be one face)
                var _ext = Orientation.CoverageExtents;
                if (moveTactical.TransitFaces
                    .Any(_f => !Orientation.IsFaceSnapped(_f, _ext)))
                {
                    return 0;
                }

                return GetCoverage(moveTactical);
            }
            return 0d;
        }
        #endregion

        #region public override bool HindersTransit(MovementBase movement, IGeometricRegion region)
        public override bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _srcPt = region.GetPoint3D();
                var _corners = region.AllCorners();
                var _sides = Orientation.GetPlanarPoints(AnchorFaceList.All);

                // if source is in a cell that is cut by a plane...
                return (from _s in _sides
                        from _c in _corners
                        let _i = _s.SegmentIntersection(_srcPt, _c).HasValue
                        where _i
                        select _i).Any();
            }
            return false;
        }
        #endregion

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        public override bool IsHardSnap(AnchorFace face)
            => false;

        protected override bool IsCoveredFace(AnchorFace face)
            => false;

        public override IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // NOTE: supported surface *might* provide more than one opens-towards...?
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _bottom = Orientation.GravityFace;
                switch (Orientation.Upright)
                {
                    case Verticality.Upright:
                        if (ApplyOpening(occupyCell, testObj, _bottom.GetAxis()))
                        {
                            // NOTE: height constrained to 5' high
                            // TODO: displacement
                            yield return new MovementOpening(_bottom, Height, 0); // TODO: blockage
                            yield return new MovementOpening(_bottom.ReverseFace(), Height, 0); // TODO: blockage
                        }
                        break;

                    case Verticality.Inverted:
                        if (ApplyOpening(occupyCell, testObj, _bottom.GetAxis()))
                        {
                            yield return new MovementOpening(_bottom.ReverseFace(), 0, 0); // TODO: displaceement, blockage
                        }
                        break;

                    default:
                        // TODO:
                        break;
                }
            }
            yield break;
        }

        protected override bool SegmentIntersection(in TacticalInfo tacticalInfo)
        {
            foreach (var _pp in Orientation.GetPlanarPoints(AnchorFaceList.All)) // TODO: ???
            {
                if (_pp.SegmentIntersection(tacticalInfo.SourcePoint, tacticalInfo.TargetPoint).HasValue)
                {
                    return true;
                }
            }
            return false;
        }

        // IAlterLink

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return Orientation.IsFaceSnapped(_face);
        }
        #endregion

        public double AllowLightFactor(LocalLink link)
            => 1 - GetCoverage(link);

        public int GetExtraSoundDifficulty(LocalLink link)
            => CanAlterLink(link)
            ? Convert.ToInt32(GetCoverage(link) * ExtraSoundDifficulty.EffectiveValue)
            : 0;
    }
}
