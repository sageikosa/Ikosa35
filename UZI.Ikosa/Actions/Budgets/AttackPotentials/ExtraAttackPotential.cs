using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used for extra attacks from haste</summary>
    [Serializable]
    public class ExtraAttackPotential : IAttackPotential
    {
        // TODO: IAttackPotentialFactory adjunct...

        /// <summary>Used for extra attacks from haste</summary>
        public ExtraAttackPotential(object source)
        {
            _Used = false;
            _Source = source;
            _Budget = null;
        }

        #region state
        private bool _Used;     // the extra attack was used
        private object _Source;
        private CoreActionBudget _Budget;
        #endregion

        public bool IsUsed => _Used;
        public string Name => @"Extra attack";
        public string Description => @"One extra attack at full attack bonus";
        public object Source => _Source;

        /// <summary>Extra attack doesn't have any use-specific Deltas</summary>
        public IDelta Delta { get => null; }

        /// <summary>Extra attack doesn't block any other attack</summary>
        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
            => false;

        /// <summary>Not used and nothing blocks this</summary>
        public bool CanUse(AttackActionBase attack)
            => !_Used
            && !_Budget.BudgetItems.Items.OfType<IAttackPotential>().Any(_bi => _bi.BlocksUse(attack, this));

        public bool RegisterUse(AttackActionBase attack)
        {
            if (CanUse(attack))
            {
                _Used = true;
                return true;
            }
            return false;
        }

        /// <summary>True means remove budget after reset</summary>
        public bool Reset()
            => true;

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
        }

        public void Removed()
        {
            // nothing other effects
        }
    }
}
