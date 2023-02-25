using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable, SourceInfo(@"Attack Sequence")]
    public class SequencePotential : ISlotAttackPotential
    {
        #region construction
        public SequencePotential(ItemSlot holdingSlot, int sequenceCount)
        {
            _Slot = holdingSlot;
            _Used = false;

            // attack sequence penalties
            _Sequence = new Queue<IDelta>();
            for (var _s = 0; _s < sequenceCount; _s++)
            {
                // 0, -5, -10, -15
                var _penalty = 1 - ((_s * 5) + 1);
                _Sequence.Enqueue(new Delta(_penalty, typeof(SequencePotential)));
            }
        }
        #endregion

        #region state
        private CoreActionBudget _Budget;
        private ItemSlot _Slot;
        private Queue<IDelta> _Sequence;
        private bool _Used;
        #endregion

        // IAttackPotential Members
        public bool CanUse(AttackActionBase attack)
            // have sequence left, the weapon head has an assignment for our slot, and nothing blocks our use of the head or potential
            => _Sequence.Any()
                && (attack.WeaponHead?.AssignedSlots.Any(_s => _s == _Slot) ?? false)
                && !_Budget.BudgetItems.Items.OfType<IAttackPotential>().Any(_bi => _bi.BlocksUse(attack, this));

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

        public bool IsUsed => _Used;

        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
        {
            // NOTE: speed and haste will be used before sequence, so we don't have to worry about a conflict here
            if (IsUsed)
            {
                // sequence was used
                if (!_Sequence.Any()
                    && (attack.WeaponHead?.AssignedSlots.Any(_s => _s == _Slot) ?? false))
                {
                    // no sequence left, and weapon head trying to use this slot
                    return true;
                }

                if ((potential != this)
                    && ((potential as ISlotAttackPotential)?.ItemSlots.Any(_s => _s == _Slot) ?? false))
                {
                    // not checking ourself, and the potential has a slot conflict
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<ItemSlot> ItemSlots
            => _Slot.ToEnumerable();

        public IDelta Delta
            => _Sequence.Any() ? _Sequence.Peek() : null;

        // IBudgetItem Members
        public string Name => $@"Sequence for {_Slot.SlotType} ({_Slot.SubType})";
        public string Description => @"Attack Sequence Capacity and Penalties";

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
        }

        public void Removed() { }

        // ISourcedObject Members
        public object Source => _Slot;

        // IResetBudgetItem Members
        public bool Reset()
            => true;
    }
}