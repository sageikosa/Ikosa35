using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FlexibleFlatPanel : FlatPanel, IAlterLocalLink, IObjectStateModels
    {
        #region ctor()
        public FlexibleFlatPanel(string name, IFlatObjectSide front, IFlatObjectSide back)
            : base(name, front, back)
        {
            _State = FlexibleFlatState.Flat;
            _Native = (0d, 0d, 0d);
            _StateKeys = new Dictionary<FlexibleFlatState, ObjectStateModelKey>
            {
                {
                    FlexibleFlatState.Flat,
                    new ObjectStateModelKey(nameof(FlexibleFlatState.Flat)) { ModelKey = @"flex_flat" }
                },
                {
                    FlexibleFlatState.Rolled,
                    new ObjectStateModelKey(nameof(FlexibleFlatState.Rolled)) { ModelKey = @"flex_rolled" }
                },
                {
                    FlexibleFlatState.Balled,
                    new ObjectStateModelKey(nameof(FlexibleFlatState.Balled)) { ModelKey = @"flex_balled" }
                },
                {
                    FlexibleFlatState.Folded,
                    new ObjectStateModelKey(nameof(FlexibleFlatState.Folded)) { ModelKey = @"flex_folded" }
                }
            };
        }
        #endregion

        #region state
        private FlexibleFlatState _State;
        private Dictionary<FlexibleFlatState, ObjectStateModelKey> _StateKeys;

        /// <summary>when flex state if not flat, original parameters are saved here</summary>
        private (double Width, double Length, double Height) _Native;
        #endregion

        public override bool CanFlip => FlexState == FlexibleFlatState.Flat;
        public override bool IsUprightAllowed => FlexState != FlexibleFlatState.Flat;

        protected void SetModelKeyByFlexState()
        {
            // should have it, but double check
            if (_StateKeys.TryGetValue(_State, out var _mKey))
            {
                // pop any existing one
                Adjuncts.OfType<OverrideModelKey>().FirstOrDefault(_omk => _omk.Source == this)?.Eject();

                // add new one
                AddAdjunct(new OverrideModelKey(this, _mKey.ModelKey));
            }
        }

        public string FlatModelKey
        {
            get => _StateKeys[FlexibleFlatState.Flat].ModelKey;
            set => _StateKeys[FlexibleFlatState.Flat].ModelKey = value;
        }

        public string FoldedModelKey
        {
            get => _StateKeys[FlexibleFlatState.Folded].ModelKey;
            set => _StateKeys[FlexibleFlatState.Folded].ModelKey = value;
        }

        public string RolledModelKey
        {
            get => _StateKeys[FlexibleFlatState.Rolled].ModelKey;
            set => _StateKeys[FlexibleFlatState.Rolled].ModelKey = value;
        }

        public string BalledModelKey
        {
            get => _StateKeys[FlexibleFlatState.Balled].ModelKey;
            set => _StateKeys[FlexibleFlatState.Balled].ModelKey = value;
        }

        #region public FlexibleFlatObjectState FlexState { get; set; }
        public FlexibleFlatState FlexState
        {
            get => _State;
            set
            {
                // no processing if no change
                if (FlexState != value)
                {
                    if (FlexState != FlexibleFlatState.Flat)
                    {
                        // restore
                        _State = FlexibleFlatState.Flat;
                        base.Length = _Native.Length;
                        base.Width = _Native.Width;
                        base.Height = _Native.Height;
                    }

                    // capture flat state dimensions
                    _Native = (Width, Length, Height);
                    switch (value)
                    {
                        case FlexibleFlatState.Folded:
                            _State = value;
                            SetUnrestrictedLength(Width / 8d);
                            base.Width = Width / 4d;
                            base.Height = Height / 4d;
                            break;

                        case FlexibleFlatState.Balled:
                            _State = value;
                            SetUnrestrictedLength((Width + Height) / 4d);
                            base.Width = Width / 3d;
                            base.Height = Height / 3d;
                            break;

                        case FlexibleFlatState.Rolled:
                            _State = value;
                            SetUnrestrictedLength(Width / 4d);
                            base.Width = Width / 4d;
                            base.Height = Math.Min(Height, 5d);
                            break;

                        default:
                            break;
                    }
                    SetModelKeyByFlexState();
                }
            }
        }
        #endregion

        #region public override double Length { get; set; }
        public override double Length
        {
            get => base.Length;
            set
            {
                if (FlexState == FlexibleFlatState.Flat)
                {
                    if ((value > 0) && (value <= 0.1d))
                    {
                        base.Length = value;
                    }
                }
                else
                {
                    SetUnrestrictedLength(value);
                }
            }
        }
        #endregion

        public override object Clone()
        {
            var _clone = new FlexibleFlatPanel(Name, Front.Clone() as IFlatObjectSide, Back.Clone() as IFlatObjectSide)
            {
                Opacity = Opacity
            };
            _clone.CopyFrom(this);
            return _clone;
        }

        public override bool IsHardSnap(AnchorFace face)
            => (FlexState != FlexibleFlatState.Flat) || base.IsHardSnap(face);

        protected override AnchorFaceList GetCoveredFaces()
            => (FlexState != FlexibleFlatState.Flat)
            ? AnchorFaceList.All
            : base.GetCoveredFaces();

        protected override AnchorFaceList GetPlanarFaces()
            => (FlexState != FlexibleFlatState.Flat)
            ? AnchorFaceList.All
            : base.GetPlanarFaces();

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public override bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return Orientation.IsFaceSnapped(_face)
                && ((FlexState != FlexibleFlatState.Flat)
                || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Back) == _face)
                || (Orientation.GetAnchorFaceForSideIndex(SideIndex.Front) == _face));
        }
        #endregion

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
                if (FlexState == FlexibleFlatState.Flat)
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
                else
                {
                    // just like regular furnishing
                    return Orientation.GetPlanarPoints(GetPlanarFaces())
                        .Any(_f => _f.SegmentIntersection(
                            moveTactical.SourceCell.GetPoint(), moveTactical.TargetCell.GetPoint()).HasValue);
                }
            }
            return false;
        }
        #endregion

        protected override string ClassIconKey =>
            @"flex_flat";

        public IEnumerable<ObjectStateModelKey> StateModelKeys
            => _StateKeys.Select(_kvp => _kvp.Value).OrderBy(_osmk => _osmk.StateKey);
    }
}
