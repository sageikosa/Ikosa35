using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class StandingShelves : Furnishing, IAlterLocalLink
    {
        public StandingShelves(Items.Materials.Material material, bool backBlock, bool bottomBlock)
            : base(@"Shelves", material)
        {
            _Faces = AnchorFaceList.ZHigh.Union(AnchorFaceList.YHigh).Union(AnchorFaceList.YLow);
            if (backBlock)
            {
                _Faces = _Faces.Union(AnchorFaceList.XLow);
            }

            if (bottomBlock)
            {
                _Faces = _Faces.Union(AnchorFaceList.ZLow);
            }
            // TODO: multiple surface containers...
        }

        #region data
        // NOTE: these flags indicate that the configuration can do these (not necessarily the material, except for Detect?)
        private bool _MoveBlock = true;
        private double _Opacity = 1d;
        // other stuff
        private int _Shelves;
        private AnchorFaceList _Faces;
        #endregion

        public override bool IsUprightAllowed => true;

        public override object Clone()
        {
            var _clone = new StandingShelves(ObjectMaterial, HasBack, HasBottom)
            {
                Opacity = Opacity,
                BlocksMove = BlocksMove
            };
            _clone.CopyFrom(this);
            return _clone;
        }

        #region public bool BlocksMove { get; set; }
        public bool BlocksMove
        {
            get { return _MoveBlock; }
            set
            {
                _MoveBlock = value;
                DoPropertyChanged(nameof(BlocksMove));
            }
        }
        #endregion

        #region public double Opacity { get; set; }
        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                _Opacity = value;
                DoPropertyChanged(nameof(Opacity));
            }
        }
        #endregion

        #region public bool HasBack { get; set; }
        public bool HasBack
        {
            get { return AnchorFaceList.XLow.IsSubset(_Faces); }
            set
            {
                _Faces = value ? _Faces.Union(AnchorFaceList.XLow) : _Faces.Remove(AnchorFaceList.XLow);
                DoPropertyChanged(nameof(HasBack));
            }
        }
        #endregion

        #region public bool HasBottom { get; set; }
        public bool HasBottom
        {
            get { return AnchorFaceList.ZLow.IsSubset(_Faces); }
            set
            {
                _Faces = value ? _Faces.Union(AnchorFaceList.ZLow) : _Faces.Remove(AnchorFaceList.ZLow);
                DoPropertyChanged(nameof(HasBottom));
            }
        }
        #endregion

        protected override AnchorFaceList GetPlanarFaces()
            => _Faces;

        protected override bool SegmentIntersection(in TacticalInfo tacticalInfo)
        {
            foreach (var _pp in Orientation.GetPlanarPoints(GetPlanarFaces())) // TODO: shelves
            {
                if (_pp.SegmentIntersection(tacticalInfo.SourcePoint, tacticalInfo.TargetPoint).HasValue)
                {
                    return true;
                }
            }
            return false;
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

        public override bool IsHardSnap(AnchorFace face)
            => (Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Left) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Right) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Back) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Bottom) == face);

        #region protected override bool IsCoveredFace(AnchorFace face)
        protected override bool IsCoveredFace(AnchorFace face)
            => (Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Left) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Right) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Back) == face)
            || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Bottom) == face);
        #endregion

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return (Orientation.IsFaceSnapped(_face)
                && IsCoveredFace(_face));
        }
        #endregion

        public double AllowLightFactor(LocalLink link)
            => (Opacity > 0d)
            ? 1 - (Opacity * GetCoverage(link))
            : 1d;

        public int GetExtraSoundDifficulty(LocalLink link)
            => CanAlterLink(link)
            ? Convert.ToInt32(GetCoverage(link) * ExtraSoundDifficulty.EffectiveValue)
            : 0;

        public override bool BlocksTransit(MovementTacticalInfo moveTactical)
            => BlocksMove
            && !moveTactical.Movement.CanMoveThrough(ObjectMaterial)
            && Orientation.GetPlanarPoints(GetPlanarFaces())
                .Any(_f => _f.SegmentIntersection(
                    moveTactical.SourceCell.GetPoint(), moveTactical.TargetCell.GetPoint()).HasValue);

        #region public override bool HindersTransit(MovementBase movement, IGeometricRegion region)
        public override bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            if (BlocksMove && !movement.CanMoveThrough(ObjectMaterial))
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

        public override IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // NOTE: supported surface *might* provide more than one opens-towards...?
            if (BlocksMove && !movement.CanMoveThrough(ObjectMaterial))
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

        #region public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        /// <summary>Used to determine support against gravity and starting elevations</summary>
        public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        {
            // TODO: influenced!
            if (BlocksMove && !moveTactical.Movement.CanMoveThrough(ObjectMaterial))
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

        #region public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        {
            if (BlocksMove && !base.CanOccupy(movement, region, testObj))
            {
                // TODO: if base allows it we're probably OK
                return false;
            }
            return true;
        }
        #endregion

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        public override IEnumerable<string> IconKeys
        {
            get
            {
                // provide any overrides
                foreach (var _iKey in IconKeyAdjunct.GetIconKeys(this))
                {
                    yield return _iKey;
                }

                // material class combination
                yield return $@"{ObjectMaterial?.Name}_{ClassIconKey}_3";

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }

        protected override string ClassIconKey
            => nameof(StandingShelves);
    }
}
