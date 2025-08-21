using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Blocks or limits awareness of invisible things</summary>
    [Serializable]
    public class InvisibilityHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Observe _obs
                && _obs.Viewer is ISensorHost _viewer
                && _viewer.IsSensorHostActive
                && workSet.Target is ICoreObject _target
                && !((_target as IVisible)?.IsVisible ?? true))
            {
                var _tPlanar = _obs.GetTargetLocator(_target)?.PlanarPresence ?? PlanarPresence.None;
                var _distance = _obs.GetDistance(_target);

                // check if not in range of usable invisibility detector
                var _inRangeDetector = _viewer.Senses.AllSenses
                    .Any(_s => _s.IsActive
                    && _s.IgnoresInvisibility
                    && (_distance <= _s.Range)
                    && _s.PlanarPresence.HasOverlappingPresence(_tPlanar));
                if (!_inRangeDetector)
                {
                    // not in range of a detector
                    // check if there is planar compatible sense in presence range
                    if ((_distance <= 30d)
                        && _viewer.Senses.AllSenses.Any(_s
                            => _s.IsActive
                            && (_distance <= _s.Range)
                            && _s.PlanarPresence.HasOverlappingPresence(_tPlanar)))
                    {
                        // if so, see if we've calculated awareness already
                        if ((_viewer?.Awarenesses?[_target.ID] ?? AwarenessLevel.None) == AwarenessLevel.None)
                        {
                            // if not, let a check be made
                            if (workSet.Target is Creature _tCreature)
                            {
                                // NOTE: using only basic spot difficulty, could be 30 or 40?
                                //       by making the level be "unaware", you only get one chance to detect per entry into 30' range
                                workSet.Feedback.Add(new ObserveSpotFeedback(this, 20,
                                    BuildBlockedIVisibles(_target, AwarenessLevel.Presence),
                                    BuildBlockedIVisibles(_target, AwarenessLevel.UnAware)));
                                return;
                            }
                            else
                            {
                                // NOTE: assuming an immobile object
                                workSet.Feedback.Add(new ObserveSpotFeedback(this, 40,
                                    BuildBlockedIVisibles(_target, AwarenessLevel.Presence),
                                    BuildBlockedIVisibles(_target, AwarenessLevel.UnAware)));
                                return;
                            }
                        }
                    }

                    // done!
                    workSet.Feedback.Add(new ObserveFeedback(this, BuildBlockedIVisibles(_target, AwarenessLevel.None)));

                    // NOTE: if there was an in-range detector, the object may still get to make a hide check...(if its a creature)                            
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Observe);
            yield return typeof(ObserveDetails);
            yield return typeof(SearchData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (typeof(ObserveHandler).IsAssignableFrom(existingHandler.GetType()))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region private Dictionary<Guid, AwarenessLevel> BuildBlockedIVisibles(CoreObject target, AwarenessLevel awareness)
        private Dictionary<Guid, AwarenessLevel> BuildBlockedIVisibles(ICoreObject target, AwarenessLevel awareness)
        {
            var _dictionary = new Dictionary<Guid, AwarenessLevel>
            {
                { target.ID, awareness }
            };
            foreach (IVisible _iVis in target.Adjuncts.Where(e => e is IVisible))
            {
                _dictionary.Add(_iVis.ID, AwarenessLevel.None);
            }
            return _dictionary;
        }
        #endregion
    }
}
