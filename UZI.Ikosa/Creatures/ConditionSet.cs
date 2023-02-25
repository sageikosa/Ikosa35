using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Conditions
    /// </summary>
    [Serializable]
    public class ConditionSet : IEnumerable<Condition>, IControlChange<Condition>, INotifyCollectionChanged, ICreatureBound
    {
        #region ctor()
        public ConditionSet(Creature critter)
        {
            _Critter = critter;
            _Conditions = new Collection<Condition>();
            _ChangeCtrl = new ChangeController<Condition>(this, null);
        }
        #endregion

        #region data
        private Creature _Critter;
        private Collection<Condition> _Conditions;
        private ChangeController<Condition> _ChangeCtrl;
        #endregion

        /// <summary>Only sets if null</summary>
        internal void SetCreature(Creature critter)
        {
            _Critter = _Critter ?? critter;
        }

        #region public Condition this[string name, object source] { get; }
        public Condition this[string name, object source]
            => _Conditions
            .FirstOrDefault(_cond => _cond.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            && (_cond.Source == source));
        #endregion

        public Creature Creature => _Critter;

        public IEnumerable<Condition> GetConditions(string name)
            => _Conditions.Where(_cond => _cond.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public bool Contains(string name)
            => _Conditions.Any(_cond => _cond.Name == name);

        #region public void Remove(Condition condition)
        public void Remove(Condition condition)
        {
            if (_Conditions.Contains(condition))
            {
                if (!_ChangeCtrl.WillAbortChange(condition, @"Remove"))
                {
                    var _index = _Conditions.IndexOf(condition);
                    _ChangeCtrl.DoPreValueChanged(condition, @"Remove");
                    _Conditions = new Collection<Condition>(_Conditions.Where(_c => _c != condition).ToList());
                    _ChangeCtrl.DoValueChanged(condition, @"Remove");
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, condition, _index));
                    if (!_Conditions.Any(_c => _c.Name == condition.Name))
                    {
                        // only report ending if no other similar conditions still exist
                        Creature?.SendSysNotify(new ConditionNotify(Creature.ID, condition.Display, true));
                    }
                }
            }
        }
        #endregion

        #region public void Add(Condition condition)
        public void Add(Condition condition)
        {
            if (!_Conditions.Contains(condition))
            {
                if (!_ChangeCtrl.WillAbortChange(condition, @"Add"))
                {
                    _ChangeCtrl.DoPreValueChanged(condition, @"Add");
                    _Conditions = new Collection<Condition>(_Conditions.Union(condition.ToEnumerable()).ToList());
                    _ChangeCtrl.DoValueChanged(condition, @"Add");
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, condition));
                    if (_Conditions.Count(_c => _c.Name == condition.Name) == 1)
                    {
                        // only notify if this is a singleton, subsequent applications of condition do not need to notify
                        Creature?.SendSysNotify(new ConditionNotify(Creature.ID, condition.Display, false));
                    }
                }
            }
        }
        #endregion

        #region INotifyCollectionChanged Members
        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        public IEnumerator<Condition> GetEnumerator()
            => _Conditions.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => _Conditions.GetEnumerator();

        #region IControlChange<Condition> Members
        public void AddChangeMonitor(IMonitorChange<Condition> monitor)
        {
            _ChangeCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Condition> monitor)
        {
            _ChangeCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
