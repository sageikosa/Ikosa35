using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class CreatureRecoverHealthPointHandler: CreatureHealthPointHandler, IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            var _critter = workSet.Target as Creature;
            if ((workSet.InteractData is RecoverHealthPointData _recover) && (_critter != null))
            {
                // cannot apply healing to a dead character!
                if (!_critter.Conditions.Contains(Condition.Dead))
                {
                    // every recover recovers non-lethal damage
                    if (_critter.HealthPoints.NonLethalDamage - _recover.Amount.EffectiveValue >= 0)
                        _critter.HealthPoints.NonLethalDamage -= _recover.Amount.EffectiveValue;
                    else
                        _critter.HealthPoints.NonLethalDamage = 0;

                    // see if lethal damage is affected
                    if (!_recover.NonLethalOnly)
                    {
                        if (_critter.HealthPoints.CurrentValue + _recover.Amount.EffectiveValue < _critter.HealthPoints.TotalValue)
                            _critter.HealthPoints.CurrentValue += _recover.Amount.EffectiveValue;
                        else
                            _critter.HealthPoints.CurrentValue = _critter.HealthPoints.TotalValue;
                    }

                    // adjust actor state
                    _critter.HealthPoints.DoRecovery();
                }
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(RecoverHealthPointData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // indicates this is the last handler
            return false;
        }
        #endregion
    }
}
