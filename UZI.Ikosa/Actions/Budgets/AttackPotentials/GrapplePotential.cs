using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    [Serializable, SourceInfo(@"Grapple Sequence")]
    public class GrapplePotential : IAttackPotential
    {
        public GrapplePotential(Creature creature, int sequenceCount)
        {
            _Critter = creature;
            _Used = false;

            // attack sequence penalties
            _Sequence = new Queue<IDelta>();
            for (var _s = 0; _s < sequenceCount; _s++)
            {
                // 0, -5, -10, -15
                var _penalty = 1 - ((_s * 5) + 1);
                _Sequence.Enqueue(new Delta(_penalty, typeof(GrapplePotential)));
            }
        }

        #region state
        private Creature _Critter;
        private CoreActionBudget _Budget;
        private Queue<IDelta> _Sequence;
        private bool _Used;
        #endregion

        public string Name => @"Sequence for Grappling";
        public string Description => @"Grapple sequence capacity and penalties";
        public object Source => _Critter;
        public bool IsUsed => _Used;
        public IDelta Delta => _Sequence.Any() ? _Sequence.Peek() : null;

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
        }

        public void Removed() { }

        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
        {
            if (IsUsed)
            {
                // TODO: improved grab and constrict alter options and effects...
                // TODO: creatures using natural weapons to grapple do not get additional grapple checks per full attack

                //// sequence was used
                //if (!_Sequence.Any()
                //    && (attack.WeaponHead?.AssignedSlots.Any(_s => _s == _Slot) ?? false))
                //{
                //    // no sequence left, and weapon head trying to use this slot
                //    return true;
                //}

                //if ((potential != this)
                //    && ((potential as ISlotAttackPotential)?.ItemSlots.Any(_s => _s == _Slot) ?? false))
                //{
                //    // not checking ourself, and the potential has a slot conflict
                //    return true;
                //}
            }
            return false;
        }

        public bool CanUse(AttackActionBase attack)
        {
            return _Sequence.Any();
                //&& (attack.WeaponHead?.AssignedSlots.Any(_s => _s == _Slot) ?? false)
                //&& !_Budget.BudgetItems.Items.OfType<IAttackPotential>().Any(_bi => _bi.BlocksUse(attack, this));
        }

        public bool RegisterUse(AttackActionBase attack)
        {
            if (CanUse(attack))
            {
                _Used = true;
                _Sequence.Dequeue();
                return true;
            }
            return false;
        }

        public bool Reset()
            => true;
    }
}
