using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class WheelManeuverabilityCost : Adjunct, IInteractHandler
    {
        public WheelManeuverabilityCost(object source, double straightFactor, double turnFactor, double orthoFactor,
            bool isSideSlide)
            : base(source)
        {
            _StraightFactor = straightFactor;
            _TurnFactor = turnFactor;
            _OrthoFactor = orthoFactor;
            _SideSlide = isSideSlide;
        }

        #region data
        private double _StraightFactor;
        private double _TurnFactor;
        private double _OrthoFactor;
        private bool _SideSlide;
        #endregion

        // TODO: ¿¿¿ forward, backward, side ways, forward turn, backward turn ???
        public double StraightFactor => _StraightFactor;
        public double TurnFactor => _TurnFactor;
        public double OrthoFactor => _OrthoFactor;

        /// <summary>True if left/right is primary movement axis</summary>
        public bool IsSideSlide => _SideSlide;

        public override object Clone()
            => new WheelManeuverabilityCost(Source, StraightFactor, TurnFactor, OrthoFactor, IsSideSlide);

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is ObjectGrabbedCostData _ogcd)
                && (workSet.Source is LandMovement))
            {
                void _makeFeedback(AnchorFaceList front, AnchorFaceList moveFaces)
                {
                    var _moveInvert = moveFaces.Invert();
                    if ((front == moveFaces) || (front == _moveInvert))
                    {
                        // heading and movement equal means straight
                        workSet.Feedback.Add(new ValueFeedback<double>(this, StraightFactor));
                    }
                    else if (front.Intersects(moveFaces) || front.Intersects(_moveInvert))
                    {
                        // otherwise, an overlap is a turn
                        workSet.Feedback.Add(new ValueFeedback<double>(this, TurnFactor));
                    }
                    else
                    {
                        // nothing in common is orthogonal
                        workSet.Feedback.Add(new ValueFeedback<double>(this, OrthoFactor));
                    }
                }

                if ((workSet.Target is Furnishing _furnish)
                    && (_furnish.Orientation.Upright == Visualize.Verticality.Upright))
                {
                    _makeFeedback(
                        _furnish.Orientation.GetAnchorFaceForSideIndex(IsSideSlide ? SideIndex.Left: SideIndex.Front).ToAnchorFaceList(),
                        _ogcd.Crossings.StripAxis(_furnish.Orientation.GravityFace.GetAxis()));
                }
                else if ((workSet.Target is Conveyance _convey)
                    && (_convey.Orientation.Upright == Visualize.Verticality.Upright))
                {
                    _makeFeedback(
                         _convey.Orientation.GetAnchorFacesForSideIndex(IsSideSlide ? SideIndex.Left : SideIndex.Front),
                        _ogcd.Crossings.StripAxis(_convey.Orientation.GravityFace.GetAxis()));
                }
                else
                {
                    workSet.Feedback.Add(new ValueFeedback<double>(this, OrthoFactor));
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectGrabbedCostData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
    }
}
