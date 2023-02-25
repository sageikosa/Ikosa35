using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class OffHandBudgetFactory : Adjunct, IAttackPotentialFactory
    {
        public OffHandBudgetFactory(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new OffHandBudgetFactory(Source);
        }

        #region IAttackPotentialFactory Members

        public IEnumerable<IAttackPotential> GetIAttackPotentials(FullAttackBudget budget)
        {
            // add multi-weapon budget if adding potential to attack
            var _mwBudget = new MultiWeaponBudgetItem();
            budget.Budget.BudgetItems.Add(_mwBudget.Source, _mwBudget);

            // define a potential for each marked off hand
            var _body = budget.Body;
            if (_body != null)
            {
                var _critter = _body.Creature;
                var _count = _critter?.OffHandIterations.EffectiveValue ?? 1;

                // grappler cannot use off-hand attacks
                if (!_critter.Conditions.Contains(Condition.Grappling))
                {
                    // turning on, but none exist, add one for each off-hand
                    foreach (var _slot in from _s in _body.ItemSlots.AllSlots
                                          where ((_s is HoldingSlot) || (_s.SlotType == ItemSlot.UnarmedSlot))
                                          && _s.IsOffHand
                                          select _s)
                    {
                        yield return new SequencePotential(_slot, _count);
                    }
                }
            }
            yield break;
        }

        #endregion
    }
}
