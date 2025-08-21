using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class CreatureNoRecoverHealthPointHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            var _critter = workSet.Target as Creature;
            var _recover = workSet.InteractData as RecoverHealthPointData;
            if ((_recover != null) && (_critter != null))
            {
                // cannot apply healing to a "dead" character!
                if (!_critter.Conditions.Contains(Condition.Dead))
                {
                    // allow the recovery if flagged appropriately
                    if (_recover.NonLivingRecovery)
                    {
                        // see if lethal damage is affected
                        if (!_recover.NonLethalOnly)
                        {
                            if (_critter.HealthPoints.CurrentValue + _recover.Amount.EffectiveValue < _critter.HealthPoints.TotalValue)
                            {
                                _critter.HealthPoints.CurrentValue += _recover.Amount.EffectiveValue;
                            }
                            else
                            {
                                _critter.HealthPoints.CurrentValue = _critter.HealthPoints.TotalValue;
                            }
                        }
                    }
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
            // this should be before the standard creature recover handler
            if (existingHandler is CreatureRecoverHealthPointHandler)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
