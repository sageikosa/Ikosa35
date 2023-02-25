using System;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    /// <summary>This budget manages multi-weapon penalties during a full attack and terminates them at the end of turn</summary>
    [Serializable]
    public class MultiWeaponBudgetItem : ITurnEndBudgetItem
    {
        /// <summary>This budget manages multi-weapon penalties during a full attack and terminates them at the end of turn</summary>
        public MultiWeaponBudgetItem()
        {
        }

        private MultiWeaponDelta _Delta;

        #region ITurnEndBudgetItem Members

        /// <summary>Clean-up penalties when turn ends</summary>
        public bool EndTurn() { return true; }

        #endregion

        #region IBudgetItem Members

        public string Name { get { return @"Multi-Weapon Penalties"; } }
        public string Description { get { return @"Penalties for fighting with two or more weapons simultaneously"; } }

        public void Added(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_budget != null)
            {
                var _critter = _budget.Actor as Creature;
                if (_critter != null)
                {
                    // add multi-weapon delta to various attack deltables
                    _Delta = _critter.MultiWeaponDelta;
                    _critter.MeleeDeltable.Deltas.Add(_Delta);
                    _critter.RangedDeltable.Deltas.Add(_Delta);
                    _critter.OpposedDeltable.Deltas.Add(_Delta);
                }
            }
        }

        public void Removed()
        {
            // removed multi-weapon delta
            if (_Delta != null)
                _Delta.DoTerminate();
        }

        #endregion

        #region ISourcedObject Members

        public object Source { get { return typeof(MultiWeaponCombatChoice); } }

        #endregion
    }
}
