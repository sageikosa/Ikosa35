using System;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Restricts movement to a linear path</summary>
    [Serializable]
    public class LinearMoveBudget : ITurnEndBudgetItem
    {
        public LinearMoveBudget()
        {
            _Previous = new Collection<byte>();
        }

        #region private data
        private Collection<byte> _Previous;
        #endregion

        #region ITurnEndBudgetItem Members
        public bool EndTurn()
        {
            // must remove budget item
            return true;
        }
        #endregion

        #region IBudgetItem Members
        public void Added(CoreActionBudget budget) { }
        public void Removed() { }
        public string Name { get { return @"Linear Movement"; } }
        public string Description { get { return @"Restricted to linear movement"; } }
        #endregion

        public object Source { get { return typeof(LinearMoveBudget); } }

        public bool IsValid(StepDestinationTarget target)
        {
            if (_Previous.Count == 0)
            {
                return true;
            }
            else
            {
                //var _test = target.GetByteValue();
                // TODO: see if test faces follow pattern
            }
            return false;
        }

        public void Add(StepDestinationTarget target)
        {
            //_Previous.Add(target.GetByteValue());
        }

        public static LinearMoveBudget GetBudget(CoreActionBudget budget)
        {
            var _linearBudget = budget.BudgetItems[typeof(LinearMoveBudget)] as LinearMoveBudget;
            if (_linearBudget == null)
            {
                _linearBudget = new LinearMoveBudget();
                budget.BudgetItems.Add(_linearBudget.Source, _linearBudget);
            }
            return _linearBudget;
        }
    }
}