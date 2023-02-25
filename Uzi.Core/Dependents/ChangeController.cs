using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// Tracks IMonitorChange instances for some class that implements IControlChange.
    /// Provides multi-cast testing of abort, and notification.
    /// </summary>
    [Serializable]
    public class ChangeController<ChangeType> : IControlChange<ChangeType>, ISourcedObject
    {
        public ChangeController(object source, ChangeType seed)
        {
            _Src = source;
            _LastVal = seed;
        }

        #region data
        private List<IMonitorChange<ChangeType>> _Mons = null;
        private List<ChangeController<ChangeType>> _Ctrls = null;
        protected ChangeType _LastVal;
        protected object _Src;
        #endregion

        /// <summary>All monitors in this controller and in any chained controllers</summary>
        public IEnumerable<IMonitorChange<ChangeType>> AllMonitors()
        {
            if (_Mons != null)
                foreach (IMonitorChange<ChangeType> _monitor in _Mons.ToList())
                    yield return _monitor;
            if (_Ctrls != null)
                foreach (ChangeController<ChangeType> _ctrl in _Ctrls.ToList())
                    foreach (IMonitorChange<ChangeType> _monitor in _ctrl.AllMonitors())
                        yield return _monitor;
            yield break;
        }

        /// <summary>Chain an existing change controller to this one</summary>
        public void AddChangeController(ChangeController<ChangeType> controller)
        {
            if (_Ctrls == null)
                _Ctrls = new List<ChangeController<ChangeType>>();
            _Ctrls.Add(controller);
        }

        /// <summary>Remove a change controller from this one</summary>
        public void RemoveChangeController(ChangeController<ChangeType> controller)
        {
            if (_Ctrls != null)
            {
                _Ctrls.Remove(controller);
                if (_Ctrls.Count == 0)
                    _Ctrls = null;
            }
        }

        public ChangeType LastValue => _LastVal;
        public object Source => _Src;

        #region public void AddChangeMonitor(IMonitorChange<ChangeType> monitor)
        public void AddChangeMonitor(IMonitorChange<ChangeType> monitor)
        {
            if (_Mons == null)
                _Mons = new List<IMonitorChange<ChangeType>>();
            if (!_Mons.Contains(monitor))
            {
                _Mons.Add(monitor);
            }
        }
        #endregion

        #region public void RemoveChangeMonitor(IMonitorChange<ChangeType> monitor)
        public void RemoveChangeMonitor(IMonitorChange<ChangeType> monitor)
        {
            if (_Mons != null)
                if (_Mons.Contains(monitor))
                {
                    _Mons.Remove(monitor);
                    if (_Mons.Count == 0)
                        _Mons = null;
                }
        }
        #endregion

        /// <summary>Determines whether any IMonitorChange monitor will veto the proposed change</summary>
        public bool WillAbortChange(ChangeType newVal)
            => WillAbortChange(newVal, string.Empty);

        #region public bool WillAbortChange(ChangeType newVal, string action)
        /// <summary>Determines whether any IMonitorChange monitor will veto the proposed change</summary>
        public bool WillAbortChange(ChangeType newVal, string action)
        {
            var _abort = new AbortableChangeEventArgs<ChangeType>(newVal, this.LastValue, action);
            foreach (var _monitor in AllMonitors())
            {
                _monitor.PreTestChange(Source, _abort);
            }
            return _abort.Abort;
        }
        #endregion

        public void DoPreValueChanged(ChangeType newValue)
        {
            DoPreValueChanged(newValue, string.Empty);
        }

        public void DoPreValueChanged(ChangeType newValue, string action)
        {
            var _args = new ChangeValueEventArgs<ChangeType>(_LastVal, newValue, action);
            foreach (var _dependent in AllMonitors())
            {
                _dependent.PreValueChanged(Source, _args);
            }
        }

        public void DoValueChanged(ChangeType newValue)
        {
            DoValueChanged(newValue, string.Empty);
        }

        public void DoValueChanged(ChangeType newValue, string action)
        {
            var _args = new ChangeValueEventArgs<ChangeType>(_LastVal, newValue, action);
            if (string.IsNullOrEmpty(action))
            {
                // only track last value if not performing an action
                _LastVal = newValue;
            }
            else
            {
                // otherwise, clear out the value
                _LastVal = default(ChangeType);
            }

            foreach (var _dependent in AllMonitors().ToList())
            {
                _dependent.ValueChanged(Source, _args);
            }
        }

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
