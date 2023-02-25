using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class EnergyResistanceHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is IDeliverDamage _deliverDamage)
                && (workSet.Target is Creature _critter))
            {
                // check each energy damage total
                foreach (var _energy in _deliverDamage.GetEnergyTypes())
                {
                    var _resist = _critter.EnergyResistances[_energy].EffectiveValue;
                    if (_resist > 0)
                    {
                        var _eTotal = _deliverDamage.GetEnergy(_energy);
                        if ((_eTotal > 0) && (_resist > 0))
                        {
                            // then nullify with what is left (if any)
                            _deliverDamage.Damages.Add(new EnergyDamageData(0 - Math.Min(_resist, _eTotal), _energy, @"Resist", -1));
                        }
                    }
                }
            }
            return;
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

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler switch
            {
                TempHPDamageHandler _ => true,
                HardnessDamageReducer _ => true,
                _ => false,
            };
    }
}
