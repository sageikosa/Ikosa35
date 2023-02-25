using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class CanMoveInteractHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
            => typeof(CanMoveInteract).ToEnumerable();

        // test same plane
        protected bool CanSubstantiallyInteract(Interaction workSet, IAdjunctable target, CanMoveInteract canMove)
        {
            if (target.PathHasActiveAdjunct<Gaseous>()
                || canMove.Candidate.PathHasActiveAdjunct<Gaseous>())
            {
                // gaseous
                return false;
            }

            // neither is gaseous, so check if only one is incorporeal
            if (target.PathHasActiveAdjunct<Incorporeal>()
                ^ canMove.Candidate.PathHasActiveAdjunct<Incorporeal>())
            {
                // only one is incorporeal
                if (target.PathHasActiveAdjunct<GhostTouchProtector>()
                    || canMove.Candidate.PathHasActiveAdjunct<GhostTouchProtector>())
                {
                    // at least one has ghost touch
                    return true;
                }

                // incorporeal
                return false;
            }

            // not gaseous, not incorporeal, or both incorporeal
            return true;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is CanMoveInteract _canMove)
            {
                // default confirms that rules applied are OK
                if (_canMove.FirstResult == null)
                {
                    if (((workSet.Target is IAdjunctable _target) && _target.GetLocated()?.Locator is Locator _tLocator)
                        && (_canMove.Candidate.GetLocated()?.Locator is Locator _cLocator))
                    {
                        if (_tLocator.PlanarPresence.HasOverlappingPresence(_cLocator.PlanarPresence))
                        {
                            if (CanSubstantiallyInteract(workSet, _target, _canMove))
                            {
                                workSet.Feedback.Add(new ValueFeedback<bool>(this, true));
                                return;
                            }
                        }
                    }

                    // no move interaction
                    workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
                    return;
                }

                // default confirms that rules applied are OK
                workSet.Feedback.Add(new ValueFeedback<bool>(this, _canMove.FirstResult ?? true));
                return;
            }

            // impossible to determine
            workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
