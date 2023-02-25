using System;
using System.Collections.Generic;
using Uzi.Ikosa.Items;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class RapidShotPotential : IAttackPotential
    {
        public RapidShotPotential(object source)
        {
            _Used = false;
            _AtkDelta = new Delta(-2, source, @"Rapid Shot");
            _Source = source;
        }

        #region state
        private bool _Used;     // the extra attack was used
        private object _Source;
        private Delta _AtkDelta;
        #endregion

        public bool IsUsed => _Used;
        public string Name => @"Rapid Shot";
        public string Description => @"One extra attack with a ranged weapon";
        public object Source => _Source;

        /// <summary>Rapid shot doesn't have any use-specific Deltas</summary>
        public IDelta Delta { get => null; }

        /// <summary>Rapid shot doesn't block any other attack</summary>
        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
            => false;

        public bool CanUse(AttackActionBase attack)
            => !_Used && (attack is IRangedSourceProvider);

        public bool RegisterUse(AttackActionBase attack)
        {
            if (CanUse(attack))
            {
                _Used = true;
                return true;
            }
            return false;
        }

        /// <summary>Rapid shot is notbound to item slots</summary>
        public IEnumerable<ItemSlot> ItemSlots { get { yield break; } }

        /// <summary>True means remove budget after reset</summary>
        public bool Reset()
            => true;

        public void Added(CoreActionBudget budget)
        {
            if (budget.Actor is Creature _critter)
            {
                _critter.RangedDeltable.Deltas.Add(_AtkDelta);
                _critter.MeleeDeltable.Deltas.Add(_AtkDelta);
                _critter.OpposedDeltable.Deltas.Add(_AtkDelta);
            }
        }

        public void Removed()
        {
            // end qualifying delta
            _AtkDelta.DoTerminate();
        }
    }
}
