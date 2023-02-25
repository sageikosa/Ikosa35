using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Uzi.Core.Contracts;
using Newtonsoft.Json;

namespace Uzi.Core
{
    [Serializable]
    public class DeltaSet : IModifier, IMonitorChange<DeltaValue>, IDependOnTerminate, IEnumerable<IModifier>
    {
        #region data
        private Dictionary<object, CDSet> _SDeltas;
        private List<IQualifyDelta> _QDeltas;
        protected object _Src;
        #endregion

        #region Construction
        public DeltaSet(object source)
        {
            _Src = source;
            _SDeltas = null;
            _QDeltas = null;
            _TCtrl = new TerminateController(this);
            _DVCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(0));
        }
        #endregion

        #region public void Add(IModifier item)
        public void Add(IModifier item)
        {
            if (item == null)
                return;

            // sourced modifier
            if (_SDeltas == null)
                _SDeltas = new Dictionary<object, CDSet>();
            if (_SDeltas.TryGetValue(item.Source, out CDSet _uStack))
            {
                // add to collection
                _uStack.Add(item);
            }
            else
            {
                // create a collection
                _uStack = new CDSet(item.Source)
                {
                    item
                };
                _SDeltas.Add(item.Source, _uStack);

                // hook change notification from collection
                _uStack.AddChangeMonitor(this);
                _uStack.AddTerminateDependent(this);
            }

            // changed!
            DoValueChanged();
        }
        #endregion

        #region public bool Remove(IModifier item)
        public bool Remove(IModifier item)
        {
            if (_SDeltas != null)
            {
                // sourced modifier collection
                if (_SDeltas.TryGetValue(item.Source, out CDSet _uStack))
                {
                    return _uStack.Remove(item);
                }
                if (_SDeltas.Count == 0)
                    _SDeltas = null;
            }
            return false;
        }
        #endregion

        #region public IEnumerable<IDelta> GetQualifiedDeltas(Qualifier source)
        public IEnumerable<IDelta> GetQualifiedDeltas(Qualifier source)
        {
            if (_QDeltas != null)
            {
                // clone the sets (make sure they are isolated)
                var _isolated = (_SDeltas == null)
                    ? new Dictionary<object, CDSet>()
                    : _SDeltas.ToDictionary(_kvp => _kvp.Key, _kvp => new CDSet(_kvp.Value));
                foreach (var _del in _QDeltas.SelectMany(_qd => _qd.QualifiedDeltas(source)).Where(_d => _d != null))
                {
                    // find set for this source...
                    if (_isolated.TryGetValue(_del.Source, out CDSet _uStack))
                    {
                        // add to set
                        _uStack.Add(_del);
                    }
                    else
                    {
                        // create an isolated set for this source...
                        _uStack = new CDSet(_del.Source, true)
                            {
                                _del
                            };
                        _isolated.Add(_del.Source, _uStack);
                    }
                }
                foreach (var _kvp in _isolated)
                {
                    yield return _kvp.Value.EffectiveDelta;
                }
            }
            else if (_SDeltas != null)
            {
                foreach (var _del in _SDeltas)
                {
                    if (_del.Value.EffectiveDelta != null)
                    {
                        yield return _del.Value.EffectiveDelta;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region Qualified Deltas
        public int QualifiedValue(Qualifier source, DeltaCalcInfo calcInfo = null)
        {
            var _log = calcInfo != null;
            var _deltas = GetQualifiedDeltas(source).ToList();
            if (_log)
            {
                foreach (var _del in _deltas)
                {
                    calcInfo.AddDelta(_del.Name, _del.Value);
                }
            }
            return _deltas.Sum(_d => _d.Value);
        }

        public void Add(IQualifyDelta qualified)
        {
            if (_QDeltas == null)
                _QDeltas = new List<IQualifyDelta>();
            if (!_QDeltas.Contains(qualified))
            {
                _QDeltas.Add(qualified);
                qualified.AddTerminateDependent(this);
            }
        }

        public void Remove(IQualifyDelta qualified)
        {
            if (_QDeltas?.Remove(qualified) ?? false)
            {
                qualified.RemoveTerminateDependent(this);
                if (_QDeltas.Count == 0)
                    _QDeltas = null;
            }
        }

        public int CountOfQualified
            => _QDeltas?.Count ?? 0;

        public IQualifyDelta GetQualified(int index)
            => _QDeltas?[index] ?? null;

        public IEnumerable<IQualifyDelta> QualifiedList
        {
            get
            {
                if (_QDeltas != null)
                    foreach (IQualifyDelta _qual in _QDeltas)
                        yield return _qual;
                yield break;
            }
        }
        #endregion

        public IDelta this[object source]
            => ((_SDeltas != null) && _SDeltas.TryGetValue(source, out var _delta))
            ? _delta
            : null;

        #region IEnumerable<IModifier> Members
        public IEnumerator<IModifier> GetEnumerator()
        {
            if (_SDeltas != null)
                foreach (var _mod in _SDeltas)
                {
                    yield return _mod.Value as IModifier;
                }
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (_SDeltas != null)
                foreach (var _mod in _SDeltas)
                {
                    yield return _mod.Value as IModifier;
                }
            yield break;
        }
        #endregion

        #region ValueChanged Event
        /// <summary>When true, change notification is suspended</summary>
        protected void DoValueChanged()
        {
            _DVCtrl.DoValueChanged(new DeltaValue(Value));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
        }

        #region IControlValue<DeltaValue> Members
        private readonly ChangeController<DeltaValue> _DVCtrl;
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _DVCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _DVCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        #region IDelta Members
        /// <summary>Sum of all deltas currently in the set</summary>
        public int Value
        {
            get
            {
                var _val = 0;
                if (_SDeltas != null)
                    foreach (var _mod in _SDeltas)
                    {
                        _val += _mod.Value.Value;
                    }
                return _val;
            }
        }

        public object Source => _Src;
        public string Name => _SDeltas.SourceName();

        public bool Enabled
        {
            get => _SDeltas?.Any(_m => _m.Value.Enabled) ?? false;
            set
            {
                // ignore
            }
        }
        #endregion

        #region Terminating Members

        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
            => _TCtrl.DoTerminate();

        #region IControlTerminate Members
        private readonly TerminateController _TCtrl;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _TCtrl?.TerminateSubscriberCount ?? 0;
        #endregion

        #endregion

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            DoValueChanged();
        }

        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }
        #endregion

        #region IDependOnTerminate Members
        public void Terminate(object sender)
        {
            // sourced modifier collection
            if (sender is CDSet _sleeStack)
            {
                if (_sleeStack.Count == 0)
                {
                    // remove sub-collection
                    _sleeStack.RemoveChangeMonitor(this);
                    _sleeStack.RemoveTerminateDependent(this);
                    _SDeltas.Remove(_sleeStack.Source);
                }
                DoValueChanged();
            }
            else
            {
                if (sender is IQualifyDelta _qual)
                {
                    Remove(_qual);
                }
            }
            return;
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
