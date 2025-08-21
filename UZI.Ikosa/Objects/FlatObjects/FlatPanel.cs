using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Represents a rigid flat panel, with front and back
    /// </summary>
    [Serializable]
    public class FlatPanel : Furnishing, IAlterLocalLink
    {
        #region ctor()
        public FlatPanel(string name, IFlatObjectSide front, IFlatObjectSide back)
            : base(name, front.ObjectMaterial)
        {
            Length = 0.1d;

            // track
            _Front = front;
            _Back = back;

            // bind
            _Front.BindToObject(this);
            _Back.BindToObject(this);
            _Front.AddChangeMonitor(this);
            _Back.AddChangeMonitor(this);

            // mark sides
            _Front.AddAdjunct(new ConnectedSides(SideIndex.Front));
            _Back.AddAdjunct(new ConnectedSides(SideIndex.Back));
        }

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new FlatPanelManipulateHandler());
            AddIInteractHandler(new GraspActionAwarenessProviderHandler());
            base.InitInteractionHandlers();
        }
        #endregion

        #region state
        // NOTE: these flags indicate that the configuration can do these (not necessarily the material, except for Detect?)
        private double _Opacity = 1d;
        private IFlatObjectSide _Front;
        private IFlatObjectSide _Back;
        #endregion

        public IFlatObjectSide Front => _Front;
        public IFlatObjectSide Back => _Back;

        public virtual bool CanFlip => true;
        public override bool IsUprightAllowed => false;

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

        protected void SetUnrestrictedLength(double length)
        {
            if ((length > 0) && (length <= 10))
            {
                base.Length = length;
            }
        }

        #region public override double Length { get; set; }
        public override double Length
        {
            get => base.Length;
            set
            {
                if ((value > 0) && (value <= 0.25d))
                {
                    base.Length = value;
                }
            }
        }
        #endregion

        public override Material ObjectMaterial { get => Front.ObjectMaterial; set { } }

        /// <summary>Synonym for Length</summary>
        public double Thickness { get => Length; set => Length = value; }

        public override object Clone()
        {
            var _clone = new FlatPanel(Name, Front.Clone() as IFlatObjectSide, Back.Clone() as IFlatObjectSide)
            {
                Opacity = Opacity
            };
            _clone.CopyFrom(this);
            return _clone;
        }

        public override bool IsHardSnap(AnchorFace face)
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Front) == face
            || Orientation.GetAnchorFaceForSideIndex(SideIndex.Back) == face;

        protected override AnchorFaceList GetCoveredFaces()
            => Orientation.GetAnchorFaceForSideIndex(SideIndex.Back).ToAnchorFaceList()
            .Add(Orientation.GetAnchorFaceForSideIndex(SideIndex.Front));

        protected override AnchorFaceList GetPlanarFaces()
            => AnchorFaceList.XHigh;

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        #region public virtual bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public virtual bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return Orientation.IsFaceSnapped(_face)
                && ((Orientation.GetAnchorFaceForSideIndex(SideIndex.Back) == _face)
                || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Front) == _face));
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
                var _side = Orientation.GetAnchorFaceForSideIndex(SideIndex.Bottom);
                var _sAxis = _side.GetAxis();

                // block if a side is blocked
                if (HedralTransitBlocking(moveTactical) > 0.4)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        protected override string ClassIconKey =>
            @"flat_panel";

        // IAnchorage Members overrides
        public override bool CanAcceptAnchor(IAdjunctable newAnchor)
            => ((newAnchor == _Front) || (newAnchor == _Back))
            && base.CanAcceptAnchor(newAnchor);

        public override bool CanEjectAnchor(IAdjunctable existingAnchor)
            => false;

    }
}
