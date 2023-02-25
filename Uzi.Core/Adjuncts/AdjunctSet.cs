using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Core
{
    /// <summary>
    /// Handles anchoring, activating, and deactivating adjuncts to an adjunct anchor as a generic collection
    /// </summary>
    [Serializable]
    public class AdjunctSet : IEnumerable<Adjunct>, INotifyPropertyChanged
    {
        private readonly IAdjunctable _Anchor;
        private readonly List<Adjunct> _Adjuncts;

        #region Constructor
        public AdjunctSet(IAdjunctable owner)
            : base()
        {
            _Anchor = owner;
            _Adjuncts = new List<Adjunct>();
        }
        #endregion

        public IAdjunctable Anchor => _Anchor;

        /// <summary>Used for WPF binding</summary>
        public IEnumerable<Adjunct> List
            => _Adjuncts.ToList();

        public IEnumerable<Adjunct> Unprotected
            => _Adjuncts.Where(_a => !_a.IsProtected).ToList();

        #region public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        /// <summary>Provides all actions implied by adjuncts.</summary>
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            foreach (var _actProv in from e in _Adjuncts
                                     where e.IsActive && (e is IActionProvider)
                                     select e as IActionProvider)
            {
                foreach (var _baseAct in _actProv.GetActions(budget))
                    yield return _baseAct;
            }
            yield break;
        }
        #endregion

        #region internal bool Add(Adjunct item)
        internal bool Add(Adjunct item)
        {
            _Adjuncts.Add(item);
            item.Anchor = _Anchor;
            if (item.Anchor == _Anchor)
            {
                // make sure it got added before we notify
                item.IsActive = item.InitialActive;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(List)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unprotected)));
                return true;
            }
            else
            {
                _Adjuncts.Remove(item);
            }
            return false;
        }
        #endregion

        #region internal bool Remove(Adjunct item)
        /// <summary>
        /// Last chance for Adjunct itself to reject the unanchor, then it is removed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal bool Remove(Adjunct item)
        {
            if (item.CanUnAnchor())
            {
                item.IsActive = false;
                var _ret = _Adjuncts.Remove(item);
                item.Anchor = null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(List)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unprotected)));
            }

            // make sure it was removed before we notify
            return (item.Anchor == null);
        }
        #endregion

        public bool Contains(Adjunct item) => _Adjuncts.Contains(item);
        public int Count => _Adjuncts.Count;

        public void EjectAll<T>() where T : Adjunct
        {
            foreach (var _a in this.OfType<T>().ToList())
                _a.Eject();
        }

        public void EjectAll<T>(T except) where T : Adjunct
        {
            foreach (var _a in this.OfType<T>().Where(_t => _t != except).ToList())
                _a.Eject();
        }

        #region IEnumerable<Adjunct> Members
        public IEnumerator<Adjunct> GetEnumerator()
        {
            foreach (Adjunct _adj in _Adjuncts)
                yield return _adj;
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (Adjunct _adj in _Adjuncts)
                yield return _adj;
            yield break;
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}