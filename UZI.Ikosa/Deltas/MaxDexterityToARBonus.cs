using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Delta that factors in encumbrance and armor weight.  
    /// Also acts as a qualifying delta to cancel out bonus when unprepared to dodge.
    /// </summary>
    [Serializable]
    public class MaxDexterityToARBonus : IModifier, IMonitorChange<DeltaValue>
    {
        #region construction
        /// <summary>
        /// Delta that factors in encumbrance and armor weight.  
        /// Also acts as a qualifying delta to cancel out bonus when unprepared to dodge.
        /// </summary>
        public MaxDexterityToARBonus(Creature creature)
        {
            // hook into dexterity
            _Dexterity = creature.Abilities.Dexterity;
            _Dexterity.AddChangeMonitor(this);
            _MaxValues = new Dictionary<object, int>();
            _Terminator = new TerminateController(this);
            _ValueCtrlr = new ChangeController<DeltaValue>(this, new DeltaValue(((IDelta)(_Dexterity)).Value));
        }
        #endregion

        void _Dexterity_ValueChanged(object sender, EventArgs e) { DoValueChanged(); }

        protected Abilities.Dexterity _Dexterity;
        protected Dictionary<object, int> _MaxValues;

        #region public int GetValue(object source)
        /// <summary>Get a maximum allowed by source</summary>
        public int GetValue(object source)
        {
            if (_MaxValues.TryGetValue(source, out var _max))
            {
                return _max;
            }
            return -1;
        }
        #endregion

        #region public void SetValue(object source, int maxDexAllowed)
        /// <summary>Set a maximum allowed</summary>
        public void SetValue(object source, int maxDexAllowed)
        {
            if (_MaxValues.ContainsKey(source))
            {
                _MaxValues[source] = maxDexAllowed;
            }
            else
            {
                _MaxValues.Add(source, maxDexAllowed);
            }
            DoValueChanged();
        }
        #endregion

        public bool Contains(object source) { return _MaxValues.ContainsKey(source); }

        #region public bool ClearValue(object source)
        /// <summary>Remove a maximum</summary>
        public bool ClearValue(object source)
        {
            bool _removed = _MaxValues.Remove(source);
            if (_removed)
            {
                DoValueChanged();
            }
            return _removed;
        }
        #endregion

        #region IDelta Members
        public int Value
        {
            get
            {
                int _minDex = ((IDelta)(_Dexterity)).Value;
                foreach (KeyValuePair<object, int> _kvp in _MaxValues)
                {
                    if (_kvp.Value < _minDex)
                    {
                        _minDex = _kvp.Value;
                    }
                }
                return _minDex;
            }
        }

        public object Source { get { return _Dexterity; } }
        public string Name { get { return @"Max Dexterity to Armor Rating"; } }

        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
                // ignore
            }
        }
        #endregion

        #region ValueChanged Event
        protected void DoValueChanged()
        {
            _ValueCtrlr.DoValueChanged(new DeltaValue(Value));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("Value"));
        }

        #region IControlChange<DeltaValue> Members
        private ChangeController<DeltaValue> _ValueCtrlr;
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
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

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}