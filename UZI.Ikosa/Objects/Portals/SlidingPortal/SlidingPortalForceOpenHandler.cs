using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SlidingPortalForceOpenHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ForceOpenData);
            yield break;
        }

        #region private AnchorFace? GetEffectiveSlideDirection(AnchorFace gravity, FaceEdge? direction, AnchorFace slidingFace, int heading, int incline)
        private AnchorFace? GetEffectiveSlideDirection(AnchorFace gravity, FaceEdge? direction, AnchorFace slidingFace,
            int heading, int incline)
        {
            if (slidingFace == gravity)
            {
                // left/right depend on heading
                // top/bottom (up/down) depend on heading/anti-heading
                if (heading % 2 == 0)
                {
                    switch (direction)
                    {
                        case FaceEdge.Bottom: return slidingFace.BackFace(heading);
                        case FaceEdge.Top: return slidingFace.FrontFace(heading);
                        case FaceEdge.Left: return slidingFace.LeftFace(heading);
                        case FaceEdge.Right: return slidingFace.RightFace(heading);
                    }
                }
            }
            else if (slidingFace == gravity.ReverseFace())
            {
                // left/right depend on heading
                // top/bottom (up/down) depend on anti-heading/heading
                if (heading % 2 == 0)
                {
                    switch (direction)
                    {
                        case FaceEdge.Bottom: return slidingFace.FrontFace(heading);
                        case FaceEdge.Top: return slidingFace.BackFace(heading);
                        case FaceEdge.Left: return slidingFace.LeftFace(heading);
                        case FaceEdge.Right: return slidingFace.RightFace(heading);
                    }
                }
            }
            else
            {
                int _heading() => gravity.GetHeadingValue(slidingFace.ToAnchorFaceList());
                switch (direction)
                {
                    case FaceEdge.Bottom: return gravity;
                    case FaceEdge.Top: return gravity.ReverseFace();
                    case FaceEdge.Left: return gravity.LeftFace(_heading());
                    case FaceEdge.Right: return gravity.RightFace(_heading());
                }
            }

            return null;
        }
        #endregion

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is ForceOpenData _forceOpen)
                && (workSet.Target is SlidingPortal _slider)
                && (workSet.Actor is Creature _critter))
            {
                // check if strength is successful...
                var _check = Deltable.GetCheckNotify(_critter.ID, @"Force Open", _slider.ID, @"Difficulty");
                if (_critter.Abilities.Strength.CheckValue(workSet, _forceOpen.GetStrengthRoll(), _check.CheckInfo)
                    >= _slider.ForceOpenDifficulty.QualifiedValue(workSet, _check.OpposedInfo))
                {
                    // direction
                    var _fType = _forceOpen.Activity.GetFirstTarget<OptionTarget>(@"Force.Type");
                    FaceEdge? _edge()
                    {
                        switch (_fType.Option.Key)
                        {
                            case @"Up": return FaceEdge.Top;
                            case @"Down": return FaceEdge.Bottom;
                            case @"Left": return FaceEdge.Left;
                            case @"Right": return FaceEdge.Right;
                            default: return null;
                        }
                    }

                    // params
                    var _actLoc = _critter.GetLocated()?.Locator;
                    var _grav = _actLoc?.GetGravityFace() ?? AnchorFace.ZLow;
                    var _sliding = _slider.AnchorFace;
                    var _towards = _slider.SlidesTowards;
                    var _hdng = _critter.Heading;
                    var _incl = _critter.Incline;

                    // use effective slide direction...and slide towards
                    if ((_slider.IsSideAccessible(true, _actLoc?.GeometricRegion)
                        && (GetEffectiveSlideDirection(_grav, _edge(), _sliding, _hdng, _incl) == _towards))
                    || (_slider.IsSideAccessible(false, _actLoc?.GeometricRegion)
                        && (GetEffectiveSlideDirection(_grav, _edge(), _sliding.ReverseFace(), _hdng, _incl) == _towards))
                        )
                    {
                        // check weight allowances also
                        if ((_towards == _grav.ReverseFace())
                            && (_slider.Weight > _critter.CarryingCapacity.LoadLiftOffGround))
                        {
                            workSet.Feedback.Add(new UnderstoodFeedback(this));
                            return;
                        }

                        // stuck!
                        var _stuck = _slider.Adjuncts.OfType<StuckAdjunct>().Where(_s => _s.IsActive).ToList();
                        if (_stuck.Any())
                        {
                            // unstick
                            foreach (var _s in _stuck)
                                _s.Eject();
                        }

                        // blocked!
                        var _blocked = _slider.Adjuncts.OfType<OpenBlocked>().Where(_b => _b.IsActive).ToList();
                        if (_blocked.Any())
                        {
                            // unblock
                            foreach (var _b in _blocked)
                            {
                                _b.ForcedOpenTarget?.DoForcedOpen();
                                _b.Eject();
                            }
                            // TODO: possibly disable/remove portal...
                        }

                        // open with valid check
                        workSet.Feedback.Add(new ValueFeedback<object>(this, _forceOpen));
                        return;
                    }
                }

                // open attempt without valid check
                workSet.Feedback.Add(new ValueFeedback<object>(this, this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
