using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class SupportedSurface : Furnishing, IAlterLocalLink
    {
        protected SupportedSurface(string name, Items.Materials.Material material)
            : base(name, material)
        {
            // TODO: surface container? trove?
        }

        #region data
        // NOTE: these flags indicate that the configuration can do these (not necessarily the material, except for Detect?)
        private double _Opacity = 1d;
        #endregion

        public override bool IsUprightAllowed => true;

        protected abstract SupportedSurface GetClone();

        public override object Clone()
        {
            var _clone = GetClone();
            _clone.Opacity = Opacity;
            _clone.CopyFrom(this);
            return _clone;
        }

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

        #region private bool AnchorMatch(AnchorFace anchor, ICellLocation cLoc)
        /// <summary>Determines if the cell location is one that contains specified anchor face</summary>
        private bool AnchorMatch(AnchorFace anchor, ICellLocation cLoc)
        {
            switch (anchor)
            {
                case AnchorFace.XHigh:
                    return ObjectPresenter.GeometricRegion.UpperX == cLoc.X;
                case AnchorFace.XLow:
                    return ObjectPresenter.GeometricRegion.LowerX == cLoc.X;
                case AnchorFace.YHigh:
                    return ObjectPresenter.GeometricRegion.UpperY == cLoc.Y;
                case AnchorFace.YLow:
                    return ObjectPresenter.GeometricRegion.LowerY == cLoc.Y;
                case AnchorFace.ZHigh:
                    return ObjectPresenter.GeometricRegion.UpperZ == cLoc.Z;
                case AnchorFace.ZLow:
                    return ObjectPresenter.GeometricRegion.LowerZ == cLoc.Z;
            }
            return false;
        }
        #endregion

        public override bool IsHardSnap(AnchorFace face)
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == face;

        protected override AnchorFaceList GetCoveredFaces()
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top).ToAnchorFaceList();

        protected override AnchorFaceList GetPlanarFaces()
            => AnchorFaceList.ZHigh;

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return (Orientation.IsFaceSnapped(_face)
                && Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == _face);
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

        #region public override bool BlocksTransit(MovementTacticalInfo moveTactical)
        public override bool BlocksTransit(MovementTacticalInfo moveTactical)
        {
            // small surfaces can be ignored
            if (Length < 2 || Width < 2)
            {
                return false;
            }

            if (!moveTactical.Movement.CanMoveThrough(ObjectMaterial))
            {
                // surface orthogonal axis
                var _topSide = Orientation.GetAnchorFaceForSideIndex(SideIndex.Top);
                var _tAxis = _topSide.GetAxis();

                // block if a side is blocked
                if (HedralTransitBlocking(moveTactical) > 0.4)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region public override IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        /// <summary>Only uses Movement and SourceCell</summary>
        public override IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // NOTE: supported surface *might* provide more than one opens-towards...?
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                // TODO: prone, crawling, hunched, or short may be able to go under and ignore the "standard elevation"
                var _loc = this.GetLocated();
                var _rgn = _loc?.Locator?.GeometricRegion;
                if (_rgn?.ContainsCell(occupyCell) ?? false)
                {
                    var _top = Orientation.GetAnchorFaceForSideIndex(SideIndex.Top);
                    if (IsCellFaceAtRegionEdge(_rgn, occupyCell, _top))
                    {
                        var _tAxis = _top.GetAxis();

                        // NOTE: treating relatively extensive surfaces as covering all...
                        var _coverage = (Length < 3) || (Width < 3)
                            ? GetCoverage(occupyCell, _tAxis)
                            : 1;
                        if (Orientation.IsFaceSnapped(_top) && ApplyOpening(occupyCell, testObj, _tAxis))
                        {
                            yield return new MovementOpening(_top.ReverseFace(), 5d, _coverage);
                        }
                        else
                        {
                            // since we are at the edge, and haven't been face-snapped ...
                            // ... it is safe to use GetCoverageExtent % 5
                            var _ext = Orientation.CoverageExtents;
                            var _cellExtent = Orientation.GetCoverageExtent(_tAxis) % 5;
                            if (ApplyOpening(occupyCell, testObj, _tAxis))
                            {
                                yield return new MovementOpening(_top, 5d - _cellExtent, _coverage);
                            }

                            //yield return new MovementOpening(_top.ReverseFace(), _cellExtent, _coverage);
                            foreach (var _mvOpen in from _af in AnchorFaceList.All.ToAnchorFaces()
                                                    where _af.GetAxis() != _tAxis
                                                    && !Orientation.IsFaceSnapped(_af, _ext)
                                                    && ApplyOpening(occupyCell, testObj, _af.GetAxis())
                                                    from _mo in AnchorFaceOpenings(_af, occupyCell, _rgn, false)
                                                    select _mo)
                            {
                                yield return _mvOpen;
                            }
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        /// <summary>Used to determine support against gravity and starting elevations</summary>
        public override double HedralTransitBlocking(MovementTacticalInfo moveTactical)
        {
            // TODO: influenced!
            if (!moveTactical.Movement.CanMoveThrough(ObjectMaterial))
            {
                // transit faces must be snapped (should only be one face)
                var _ext = Orientation.CoverageExtents;
                if (moveTactical.TransitFaces
                    .Any(_f => !Orientation.IsFaceSnapped(_f, _ext)))
                {
                    return 0;
                }

                var _bottom = Orientation.GravityFace;
                switch (Orientation.Upright)
                {
                    case Verticality.Upright:
                        // movement must be opposite to bottom face
                        if (moveTactical.TransitFaces.Any(_f => !_f.IsOppositeTo(_bottom)))
                        {
                            return 0d;
                        }

                        break;

                    case Verticality.Inverted:
                        // movement must be same as bottom face
                        if (moveTactical.TransitFaces.Any(_f => _f != _bottom))
                        {
                            return 0d;
                        }

                        break;

                    default:
                        // movement be opposite to appropriate heading-calculated face
                        var _surfFace = _bottom.FrontFace(Orientation.Heading * 2) ?? AnchorFace.XHigh;
                        if (moveTactical.TransitFaces.Any(_f => _f != _surfFace))
                        {
                            return 0d;
                        }

                        break;
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
                // snapped faces do not hinder...
                var _ext = Orientation.CoverageExtents;
                var _bottom = Orientation.GravityFace;
                switch (Orientation.Upright)
                {
                    case Verticality.Upright:
                        if (Orientation.IsFaceSnapped(_bottom.ReverseFace(), _ext))
                        {
                            return false;
                        }

                        break;

                    case Verticality.Inverted:
                        if (Orientation.IsFaceSnapped(_bottom, _ext))
                        {
                            return false;
                        }

                        break;

                    default:
                        var _surfFace = _bottom.FrontFace(Orientation.Heading * 2) ?? AnchorFace.XHigh;
                        if (Orientation.IsFaceSnapped(_surfFace, _ext))
                        {
                            return false;
                        }

                        break;
                }

                // otherwise, cut by plane will hinder
                return base.HindersTransit(movement, region);
            }
            return false;
        }
        #endregion

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }
    }
}