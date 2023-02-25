using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Applies energy resistance and damage reduction to reduce damage.
    /// </summary>
    [Serializable]
    public class DamageReductionHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            // must be a single damage to a creature from a weapon head
            if ((workSet?.InteractData is IDeliverDamage _damage)
                && (workSet?.Target is Creature _critter)
                && (workSet?.Source is IWeaponHead _weaponHead))
            {
                // DR for weapon heads
                var _maxDR = (from _dr in _critter.DamageReductions
                              where !_dr.WeaponIgnoresReduction(_weaponHead)
                              orderby _dr.Amount descending
                              select _dr).FirstOrDefault();
                if (_maxDR != null)
                {
                    // TODO: add alteration first?

                    var _lethal = _damage.GetLethalNonEnergy();
                    var _amount = _maxDR.Amount;
                    if (_lethal > 0)
                    {
                        // reduce lethal damage
                        var _reduce = Math.Min(_lethal, _amount);
                        _damage.Damages.Add(new DamageData(0 - _reduce, false, @"DR", -1));
                        _amount -= _reduce;
                        _maxDR.HasReduced(_reduce);
                    }
                    if (_amount > 0)
                    {
                        // any left, reduce non-lethal damage
                        var _nonLethal = _damage.GetNonLethal();
                        if (_nonLethal > 0)
                        {
                            var _nlReduce = Math.Min(_amount, _nonLethal);
                            _damage.Damages.Add(new DamageData(0 - _nlReduce, true, @"DR", -1));
                            _maxDR.HasReduced(_nlReduce);
                        }
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler switch
            {
                EnergyResistanceHandler _ => true,
                _ => false,
            };
        #endregion
    }
}
