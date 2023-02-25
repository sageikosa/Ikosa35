using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Creature handles SoundData</summary>
    [Serializable]
    public class SoundHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            // TODO: ISensorHost (for sound-based triggers)
            if ((workSet.InteractData is SoundData _sound)
                && (workSet.Target is ISensorHost _sensors)
                && _sensors.IsSensorHostActive)
            {
                if (!_sensors.Senses.AllSenses.Any(_sense => _sense.UsesHearing && _sense.IsActive))
                {
                    // creature has no active hearing sense, no need to check further
                    return;
                }

                // should be able to hear something, if after transitting, there is leftover range
                if (_sound.RangeRemaining > 0)
                {
                    // collect check results: only one will be sent per soundGroup
                    var _checkInfo = (new ScoreDeltable(_sound.CheckRoll, _sensors.Skills.Skill<ListenSkill>(), @"Listen Skill"))
                        ?.Score.GetDeltaCalcInfo(workSet, @"Listen Skill");
                    var _soundInfo = _sound.GetListenDifficulty();
                    if (_checkInfo.Result >= _soundInfo.Result)
                    {
                        workSet.Feedback.Add(new SoundFeedback(this, _soundInfo, _checkInfo));
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(SoundData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last in chain (deafness can be sense de-activation, or handled by a new interactor [not sure yet])
            return false;
        }
        #endregion
    }
}
