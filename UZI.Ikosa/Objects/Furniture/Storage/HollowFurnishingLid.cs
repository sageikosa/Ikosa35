using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class HollowFurnishingLid : Furnishing
    {
        protected HollowFurnishingLid(string name, Items.Materials.Material material)
            : base(name, material)
        {
            _TimeCost = new ActionTime(Contracts.TimeType.Brief);
        }

        #region data
        private ActionTime _TimeCost;
        #endregion

        public ActionTime ActionTime
        {
            get => _TimeCost;
            set
            {
                if (value != null)
                {
                    _TimeCost = value;
                    DoPropertyChanged(nameof(ActionTime));
                }
            }
        }

        public override bool IsUprightAllowed => false;

        public abstract bool IsValidLid(HollowFurnishing hollowFurnishing);

        public bool IsLidding 
            => (HollowFurnishing != null);

        public HollowFurnishing HollowFurnishing
            => Adjuncts.OfType<ObjectBound>()?
            .FirstOrDefault(_ob => _ob.Anchorage is HollowFurnishing)?
            .Anchorage as HollowFurnishing;

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_budget?.HasTime(ActionTime) ?? false)
            {
                if (IsLidding)
                {
                    yield return new RemoveLid(this, ActionTime, @"201");

                    // base actions: no independent grab when lidded
                    foreach (var _act in base.GetTacticalActions(budget))
                    {
                        if (!(_act is GrabObject))
                        {
                            yield return _act;
                        }
                    }
                }
                else
                {
                    yield return new PutLid(this, ActionTime, @"202");

                    // base actions
                    foreach (var _act in base.GetTacticalActions(budget))
                    {
                        yield return _act;
                    }
                }
            }
            yield break;
        }

        /// <summary>Returns cloud of points representing the corners of the surface</summary>
        private PlanarPoints GetSurfaceCorners()
            => Orientation.GetPlanarPoints(AnchorFace.ZHigh.ToAnchorFaceList()).FirstOrDefault();

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        public override bool BlocksTransit(MovementTacticalInfo moveTactical)
            => !moveTactical.Movement.CanMoveThrough(ObjectMaterial)
            && GetSurfaceCorners()
                .SegmentIntersection(moveTactical.SourceCell.GetPoint(), moveTactical.TargetCell.GetPoint()).HasValue;

        #region public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        public override bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        {
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                // TODO: depends on bulkiness...or on orientation...or on crawling under...on top...
                var _corners = GetSurfaceCorners();
                var _cells = region.AllCellLocations().ToList();
                while (_cells.Any())
                {
                    var _source = _cells.First();
                    foreach (var _target in _cells.Skip(1))
                    {
                        if (_corners.SegmentIntersection(_source.GetPoint(), _target.GetPoint()).HasValue)
                        {
                            return false;
                        }
                    }
                    _cells.Remove(_source);
                }
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

                var _bottom = Orientation.GravityFace;
                switch (Orientation.Upright)
                {
                    case Verticality.Upright:
                        // movement must be same as bottom face
                        if (moveTactical.TransitFaces.Any(_f => !_f.IsOppositeTo(_bottom)))
                        {
                            return 0d;
                        }

                        break;

                    case Verticality.Inverted:
                        // movement must be opposite to bottom face
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

        public override bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _srcPt = region.GetPoint3D();
                var _corners = region.AllCorners();
                var _sides = new List<PlanarPoints>();
                var _trans = Orientation.TransformGroup();
                void _addFace(AnchorFace face)
                {
                    var _pts = GetDimensionalCorners(face);
                    if (_trans.Children.Any())
                    {
                        _trans.Transform(_pts);
                    }

                    _sides.Add(new PlanarPoints(_pts));
                };
                _addFace(AnchorFace.ZHigh);

                // if source is in a cell that is cut by a plane...
                if ((from _s in _sides
                     from _c in _corners
                     let _i = _s.SegmentIntersection(_srcPt, _c).HasValue
                     where _i
                     select _i).Any())
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        public override bool IsHardSnap(AnchorFace face)
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top).GetAxis() == face.GetAxis();

        protected override bool IsCoveredFace(AnchorFace face)
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == face;

        /// <summary>Only uses Movement and SourceCell</summary>
        public override IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // NOTE: supported surface *might* provide more than one opens-towards...?
            if (!movement.CanMoveThrough(ObjectMaterial))
            {
                var _loc = this.GetLocated();
                var _rgn = _loc?.Locator?.GeometricRegion;
                if (_rgn?.ContainsCell(occupyCell) ?? false)
                {
                    var _bottom = Orientation.GravityFace;
                    switch (Orientation.Upright)
                    {
                        case Verticality.Upright:
                            if (((_bottom == AnchorFace.ZLow) && (occupyCell.Z == _rgn.UpperZ))
                                || ((_bottom == AnchorFace.YLow) && (occupyCell.Y == _rgn.UpperY))
                                || ((_bottom == AnchorFace.XLow) && (occupyCell.X == _rgn.UpperX))
                                || ((_bottom == AnchorFace.ZHigh) && (occupyCell.Z == _rgn.LowerZ))
                                || ((_bottom == AnchorFace.YHigh) && (occupyCell.Y == _rgn.LowerY))
                                || ((_bottom == AnchorFace.XHigh) && (occupyCell.X == _rgn.LowerX)))
                            {
                                var _down = Height; // TODO: axial displacement
                                var _coverage = GetCoverage(occupyCell, _bottom.GetAxis());
                                yield return new MovementOpening(_bottom, _down, _coverage);
                                yield return new MovementOpening(_bottom.ReverseFace(), 5d - _down, _coverage);
                            }
                            break;

                        case Verticality.Inverted:
                            if (((_bottom == AnchorFace.ZLow) && (occupyCell.Z == _rgn.LowerZ))
                                || ((_bottom == AnchorFace.YLow) && (occupyCell.Y == _rgn.LowerY))
                                || ((_bottom == AnchorFace.XLow) && (occupyCell.X == _rgn.LowerX))
                                || ((_bottom == AnchorFace.ZHigh) && (occupyCell.Z == _rgn.UpperZ))
                                || ((_bottom == AnchorFace.YHigh) && (occupyCell.Y == _rgn.UpperY))
                                || ((_bottom == AnchorFace.XHigh) && (occupyCell.X == _rgn.UpperX)))
                            {
                                var _up = 5; // TODO: axial displacement
                                var _coverage = GetCoverage(occupyCell, _bottom.GetAxis());
                                yield return new MovementOpening(_bottom.ReverseFace(), _up, _coverage);
                                yield return new MovementOpening(_bottom, 5d - _up, _coverage);
                            }
                            break;

                        case Verticality.OnSideTopOut:
                            // reverse top-side opening, top-side is snapped
                            break;
                    }
                }
            }
            yield break;
        }

        protected override bool SegmentIntersection(in TacticalInfo tacticalInfo)
            => GetSurfaceCorners()
            .SegmentIntersection(tacticalInfo.SourcePoint, tacticalInfo.TargetPoint).HasValue;

        protected override string ClassIconKey
            => nameof(HollowFurnishingLid);
    }
}
