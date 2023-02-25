using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Universal
{
    /// <summary>Self resetting budget item that counts for a source</summary>
    [Serializable]
    public class CounterBudgetItem : IResetBudgetItem
    {
        /// <summary>Self resetting budget item that counts for a source</summary>
        public CounterBudgetItem(object source, int max)
        {
            _Source = source;
            _Max = max;
            _Count = 0;
        }

        #region private data
        private object _Source;
        private int _Max;
        private int _Count;
        #endregion

        public bool CanUse { get { return _Count < _Max; } }
        public void Use() { _Count++; }

        public int Max { get { return _Max; } }
        public int Remaining { get { return _Max - _Count; } }

        public bool Reset() { _Count = 0; return false; }
        public object Source { get { return _Source; } }

        /// <summary>Get or create a self resetting budget item that counts for a source</summary>
        public static CounterBudgetItem GetCounter(CoreActionBudget budget, object source, int defaultMax)
        {
            var _found = (from _itm in budget.BudgetItems
                          where _itm.Key == source
                          let _ctr = _itm.Value as CounterBudgetItem
                          select _ctr).FirstOrDefault();
            if (_found == null)
            {
                // add if it had to be created
                _found = new CounterBudgetItem(source, defaultMax);
                budget.BudgetItems.Add(source, _found);
            }

            return _found;
        }

        #region IBudgetItem Members
        public void Added(CoreActionBudget budget) { }
        public void Removed() { }
        public string Name { get { return Source.SourceName(); } }
        public string Description { get { return string.Format(@"{0} of {1} remaining", Remaining, Max); } }
        #endregion
    }
}
