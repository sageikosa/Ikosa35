using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Awareness level of source (one of: None, Presence, UnAware, or Aware)
    /// </summary>
    public class ObserveFeedback : InteractionFeedback
    {
        public ObserveFeedback(object source, IEnumerable<KeyValuePair<Guid, AwarenessLevel>> levels)
            : base(source)
        {
            Levels = [];
            foreach (var _kvp in levels)
            {
                Levels.Add(_kvp.Key, _kvp.Value);
            }
        }

        public Dictionary<Guid, AwarenessLevel> Levels { get; private set; }

        #region private static IEnumerable<KeyValuePair<Guid, AwarenessLevel>> YieldObserveResults(...)
        private static IEnumerable<KeyValuePair<Guid, AwarenessLevel>> YieldObserveResults(Creature creature,
            IInteract objConnect, Observe observe)
        {
            var _cnInteract = new Interaction(null, creature, objConnect, observe);
            objConnect.HandleInteraction(_cnInteract);
            foreach (var _cnFeed in _cnInteract.Feedback.OfType<ObserveFeedback>())
            {
                if ((_cnFeed is ObserveSpotFeedback _cnSpotFeed)
                    && creature.Skills.Skill<SpotSkill>().AutoCheck(_cnSpotFeed.Difficulty, objConnect))
                {
                    // spot success
                    foreach (var _kvp in _cnSpotFeed.SpotSuccesses)
                    {
                        yield return _kvp;
                    }
                }
                else
                {
                    // regular feedback, or spot failure
                    foreach (var _kvp in _cnFeed.Levels)
                    {
                        yield return _kvp;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<KeyValuePair<Guid, AwarenessLevel>> YieldConnectedResults(...)
        public static IEnumerable<KeyValuePair<Guid, AwarenessLevel>> YieldConnectedResults(AwarenessLevel result,
            CoreObject coreObj, ObserveFeedback feedback, Creature creature, ISensorHost sensors,
            Locator targetLocator, Locator sensorLocator, double distance, bool active = false)
        {
            switch (result)
            {
                case AwarenessLevel.DarkDraw:
                    if (active)
                    {
                        // aware of located object ...
                        // ... just need a details check on connected objects
                        foreach (var _objConnect in coreObj.AllConnected(null).OfType<IInteract>())
                        {
                            var _cnDetail = new ObserveDetails(creature, sensors, targetLocator, sensorLocator, distance);
                            foreach (var _yor in YieldObserveResults(creature, _objConnect, _cnDetail))
                            {
                                yield return _yor;
                            }
                        }
                    }
                    break;

                case AwarenessLevel.Aware:
                    // aware of located object ...
                    // ... just need a details check on connected objects
                    foreach (var _objConnect in coreObj.AllConnected(null).OfType<IInteract>())
                    {
                        var _cnDetail = new ObserveDetails(creature, sensors, targetLocator, sensorLocator, distance);
                        foreach (var _yor in YieldObserveResults(creature, _objConnect, _cnDetail))
                        {
                            yield return _yor;
                        }
                    }
                    break;

                case AwarenessLevel.None:
                case AwarenessLevel.Presence:
                case AwarenessLevel.UnAware:
                default:
                    // not aware of located object ...
                    // ... if due to invisibility, check connected objects fully
                    if (feedback.Source is InvisibilityHandler)
                    {
                        foreach (var _objConnect in coreObj.AllConnected(null).OfType<IInteract>())
                        {
                            var _cnObserve = new Observe(creature, sensors, targetLocator, sensorLocator, distance);
                            foreach (var _yor in YieldObserveResults(creature, _objConnect, _cnObserve))
                            {
                                yield return _yor;
                            }
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
