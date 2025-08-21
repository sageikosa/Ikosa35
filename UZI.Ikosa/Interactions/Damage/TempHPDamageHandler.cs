using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class TempHPDamageHandler : IInteractHandler
    {
        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            var _critter = workSet.Target as Creature;

            // damage interaction?
            if (workSet.InteractData is IDeliverDamage _damage)
            {
                var _value = _damage.GetLethal();
                if ((_value > 0) && (_critter.TempHealthPoints.Total > 0))
                {
                    // apply all damage
                    if (_critter.TempHealthPoints.Total >= _value)
                    {
                        // temp hit points can absorb all damage (no more processing needed)
                        _critter.TempHealthPoints.Remove(_value);
                        _damage.Damages.Add(new DamageData(0 - _value, true, @"Temp Health", -1));
                    }
                    else
                    {
                        // remove all temp hit points, compensate damage by amount
                        var _tempHP = _critter.TempHealthPoints.Total;
                        _critter.TempHealthPoints.Remove(_tempHP);
                        _damage.Damages.Add(new DamageData(0 - _tempHP, true, @"Temp Health", -1));
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put temp HP before regular HP when applying damage
            if (typeof(CreatureNonLivingDamageHandler).IsAssignableFrom(existingHandler.GetType()))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
