using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class MechanismMountObserveHandler : ObserveHandler
    {
        public override void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is Observe _mountObs)
            {
                if ((workSet.Target is MechanismMount _mount)
                    && (workSet.Source is Creature _critter))
                {
                    // can any mechanism be observed?
                    var _mountMax = AwarenessLevel.None;
                    foreach (var _mech in _mount.Anchored.OfType<Mechanism>())
                    {
                        // directly targetted observe for mechanism
                        var _obs = new Observe(_critter, _mountObs.Viewer, _mountObs.GetTargetLocator(_mech),
                            _mountObs.ObserverLocator, _mountObs.GetDistance(_mech));
                        var _obsInteract = new Interaction(null, _critter, _mech, _obs);
                        _mech.HandleInteraction(_obsInteract);
                        foreach (var _oFeed in _obsInteract.Feedback.OfType<ObserveFeedback>())
                        {
                            if ((_oFeed is ObserveSpotFeedback _osFeed)
                                && _critter.Skills.Skill<SpotSkill>().AutoCheck(_osFeed.Difficulty, _mech))
                            {
                                // successful spot
                                var _mechMax = _osFeed.SpotSuccesses.Max(_kvp => _kvp.Value);
                                if (_mechMax > _mountMax)
                                    _mountMax = _mechMax;
                            }
                            else
                            {
                                // regular observe
                                var _mechMax = _oFeed.Levels.Max(_kvp => _kvp.Value);
                                if (_mechMax > _mountMax)
                                    _mountMax = _mechMax;
                            }
                            if (_mountMax >= AwarenessLevel.Aware)
                                break;
                        }
                        if (_mountMax >= AwarenessLevel.Aware)
                            break;
                    }

                    // return maximum observe of mechanisms
                    workSet.Feedback.Add(
                        new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[]
                        {
                            new KeyValuePair<Guid, AwarenessLevel>(_mount.ID, _mountMax)
                        }));
                }
            }
        }

        public override bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (existingHandler.GetType() == typeof(ObserveHandler))
                return true;
            return false;
        }
    }
}
