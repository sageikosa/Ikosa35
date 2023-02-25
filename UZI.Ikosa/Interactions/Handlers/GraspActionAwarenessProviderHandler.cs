using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Interaction target provides its ID as an ActionAwareness to GraspFeedback</summary>
    [Serializable]
    public class GraspActionAwarenessProviderHandler : IProcessFeedback
    {
        /// <summary>Interaction target provides its ID as an ActionAwareness to GraspFeedback</summary>
        public GraspActionAwarenessProviderHandler()
        {
        }

        #region public void ProcessFeedback(Interaction workSet)
        public void ProcessFeedback(Interaction workSet)
        {
            var _graspBack = workSet.Feedback.OfType<GraspFeedback>().FirstOrDefault();
            if (_graspBack != null)
            {
                // AllAccessible for simplicity of processing...
                var _critter = workSet.Actor as Creature;
                var _target = workSet.Target as CoreObject;
                if (_critter?.Awarenesses.GetAwarenessLevel(workSet.Target.ID) >= AwarenessLevel.Aware)
                {
                    // build list
                    var _avail = new List<Guid>
                    {
                        workSet.Target.ID
                    };

                    // look for attached things
                    var _graspSense = new GraspSense(this);
                    var _sensorLocator = _critter.GetLocated()?.Locator;
                    var _targetLocator = _target?.GetLocated()?.Locator;
                    try
                    {
                        // observation from creature to target object
                        _critter.Senses.Add(_graspSense);
                        var _obs = new Observe(_critter, _critter, _targetLocator, _sensorLocator, 5d);
                        var _obsInteract = new Interaction(null, _critter, workSet.Target, _obs);
                        workSet.Target.HandleInteraction(_obsInteract);
                        foreach (var _oFeed in _obsInteract.Feedback.OfType<ObserveFeedback>())
                        {
                            // default (or only) level
                            if ((_oFeed is ObserveSpotFeedback _osFeed)
                                && _critter.Skills.Skill<SpotSkill>().AutoCheck(_osFeed.Difficulty, _target))
                            {
                                // iterate it back
                                foreach (var _kvp in _osFeed.SpotSuccesses)
                                {
                                    _avail.Add(_kvp.Key);
                                    _avail.AddRange(
                                        ObserveFeedback.YieldConnectedResults(_kvp.Value, _target, _osFeed,
                                            _critter, _critter, _targetLocator, _sensorLocator, 5d, true)
                                        .Where(_cn => _cn.Value == AwarenessLevel.Aware || _cn.Value == AwarenessLevel.DarkDraw)
                                        .Select(_cn => _cn.Key));
                                }
                            }
                            else
                            {
                                // iterate it back
                                foreach (var _kvp in _oFeed.Levels)
                                {
                                    _avail.Add(_kvp.Key);
                                    _avail.AddRange(
                                        ObserveFeedback.YieldConnectedResults(_kvp.Value, _target, _oFeed,
                                        _critter, _critter, _targetLocator, _sensorLocator, 5d, true)
                                        .Where(_cn => _cn.Value >= AwarenessLevel.Aware || _cn.Value == AwarenessLevel.DarkDraw)
                                        .Select(_cn => _cn.Key));
                                }
                            }
                        }
                    }
                    finally
                    {
                        _critter.Senses.Remove(_graspSense);
                    }
                    _graspBack.ActionAwarnesses = _avail.Distinct().ToArray();
                }
                else
                {
                    _graspBack.ActionAwarnesses = new Guid[] { workSet.Target.ID };
                }
            }
        }
        #endregion

        public void HandleInteraction(Interaction workSet) { }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GraspData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
    }
}
