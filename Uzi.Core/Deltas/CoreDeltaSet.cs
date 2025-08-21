using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

namespace Uzi.Core
{
    /// <summary>CoreDeltaSet: Internal class used to hold modifiers for a modifier collection from the same source</summary>
    [Serializable]
    public class CDSet : ICollection<IDelta>, IList<IDelta>, IModifier, IDependOnTerminate, IMonitorChange<DeltaValue>
    {
        #region ctor()
        internal CDSet(object source, bool isolate = false)
        {
            _TCtrl = new TerminateController(this);
            _CCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(0));
            _Mods = [];
            _Src = source;
            _Isolate = isolate;
        }

        /// <summary>
        /// creates an isolated clone of the IDeltas and source
        /// </summary>
        /// <param name="copyFrom"></param>
        internal CDSet(CDSet copyFrom)
        {
            _Src = copyFrom.Source;
            _Mods = copyFrom.ToList();
            _Isolate = true;
        }
        #endregion

        #region data
        protected List<IDelta> _Mods;
        protected object _Src;
        private readonly TerminateController _TCtrl;
        private readonly bool _Isolate;
        private readonly ChangeController<DeltaValue> _CCtrl;
        #endregion

        #region protected int IndexOf(int val)
        /// <summary>Finds the insertion point needed for a new item of a particular value</summary>
        protected int IndexOf(int val)
        {
            for (var _ix = 0; _ix < _Mods.Count; _ix++)
            {
                IDelta _item = _Mods[_ix];
                if (_item.Value >= val)
                {
                    return _ix;
                }
            }
            return -1;
        }
        #endregion

        #region ICollection<IModifier> Members
        public void Add(IDelta item)
        {
            var _insPt = IndexOf(item.Value);
            if (_insPt < 0)
            {
                _Mods.Add(item);
            }
            else
            {
                _Mods.Insert(_insPt, item);
            }

            if (!_Isolate
                && item is IModifier _modifier)
            {
                // Hook changes to the modifier
                _modifier.AddChangeMonitor(this);
                _modifier.AddTerminateDependent(this);

                // notify others of the value changed
                DoValueChanged();
            }
        }

        public void Clear()
        {
            foreach (var _mod in _Mods.OfType<IModifier>())
            {
                _mod.RemoveChangeMonitor(this);
                _mod.RemoveTerminateDependent(this);
            }
            _Mods.Clear();
            DoValueChanged();
        }

        public bool Contains(IDelta item)
        {
            return _Mods.Contains(item);
        }

        public void CopyTo(IDelta[] array, int arrayIndex)
        {
            _Mods.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _Mods.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the element from the collection, informs of value changes, and possibly terminates the collection if needed.
        /// </summary>
        public bool Remove(IDelta item)
        {
            var _rmv = _Mods.Remove(item);
            if (_rmv)
            {
                if (!_Isolate
                    && item is IModifier _modifier)
                {
                    _modifier.RemoveChangeMonitor(this);
                    _modifier.RemoveTerminateDependent(this);
                    DoValueChanged();
                    if (_Mods.Count == 0)
                    {
                        // if we have nothing, we are nothing
                        DoTerminate();
                    }
                }
            }
            return _rmv;
        }
        #endregion

        public IEnumerator<IDelta> GetEnumerator()
            => _Mods.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _Mods.GetEnumerator();

        #region IList<IModifier> Members
        public int IndexOf(IDelta item)
            => _Mods.IndexOf(item);

        public void Insert(int index, IDelta item)
        {
            // ignore the index, must sort based on value of modifier
            Add(item);
        }

        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        public IDelta this[int index]
        {
            get => _Mods[index];
            set { }
        }
        #endregion

        #region ValueChanged
        protected void DoValueChanged()
        {
            _CCtrl.DoValueChanged(new DeltaValue(Value));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"Value"));
        }

        #region IControlValue<DeltaValue> Members
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber) { _CCtrl.AddChangeMonitor(subscriber); }
        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber) { _CCtrl.RemoveChangeMonitor(subscriber); }
        #endregion
        #endregion

        public IDelta EffectiveDelta
            => (Count > 0)
            ? ((this[0].Value < 0) ? this[0] : this[Count - 1])
            : null;

        #region IDelta Members
        public int Value
            => EffectiveDelta?.Value ?? 0;

        public virtual object Source => _Src;

        public virtual string Name
            => EffectiveDelta?.Name ?? string.Empty;

        public bool Enabled
        {
            get
            {
                return this.Any(_d => _d.Enabled);
            }
            set
            {
                // read only
            }
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
            => _TCtrl.DoTerminate();

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _TCtrl.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _TCtrl.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _TCtrl?.TerminateSubscriberCount ?? 0;
        #endregion
        #endregion

        #region IDependOnTerminate Members
        public void Terminate(object sender)
        {
            if (sender is IDelta _item)
            {
                Remove(_item);
            }
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }
        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }

        /// <summary>When a collected modifier changes, it needs to be rebalanced in the list by value</summary>
        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (sender is IDelta _item)
            {
                _Mods.Remove(_item);
                Add(_item);
            }
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}