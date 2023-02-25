using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class BudgetItemSet : IEnumerable<KeyValuePair<object, IBudgetItem>>
    {
        public BudgetItemSet(CoreActionBudget budget)
        {
            _Items = new Dictionary<object, IBudgetItem>();
            _Budget = budget;
        }

        #region private data
        private CoreActionBudget _Budget;
        private Dictionary<object, IBudgetItem> _Items;
        #endregion

        public IEnumerable<IBudgetItem> Items
            => _Items.Select(_kvp => _kvp.Value);

        public IBudgetItem this[object key]
            => (_Items.TryGetValue(key, out var _item))
            ? _item
            : null;

        public bool ContainsKey(object key)
            => _Items.ContainsKey(key);

        public void Add(object key, IBudgetItem item)
        {
            if (!_Items.ContainsKey(key))
            {
                _Items.Add(key, item);
                item.Added(_Budget);
            }
        }

        public void Remove(object key)
        {
            if (_Items.TryGetValue(key, out var _item))
            {
                _Items.Remove(key);
                _item.Removed();
            }
        }

        #region IEnumerable<KeyValuePair<object, IBudgetItem>> Members

        public IEnumerator<KeyValuePair<object, IBudgetItem>> GetEnumerator()
        {
            foreach (var _i in _Items)
                yield return _i;
            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _i in _Items)
                yield return _i;
            yield break;
        }

        #endregion
    }
}
