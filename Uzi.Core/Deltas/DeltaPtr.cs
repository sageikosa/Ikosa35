using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>
    /// Acts as a fixed delta to consumers, but is actually a pointer so it can have its entire delta value replaced internally.
    /// </summary>
    [Serializable]
    public class DeltaPtr : IModifier, IDependOnTerminate, IMonitorChange<DeltaValue>
    {
        #region Construction
        public DeltaPtr(IModifier current)
        {
            _Terminator = new TerminateController(this);
            _ChangeCtrlr = new ChangeController<DeltaValue>(this, new DeltaValue(current.Value));
            _CurrentDelta = current;
            _CurrentDelta.AddTerminateDependent(this);
            _CurrentDelta.AddChangeMonitor(this);
        }
        #endregion

        #region data
        protected IModifier _CurrentDelta;
        private ChangeController<DeltaValue> _ChangeCtrlr;
        private TerminateController _Terminator;
        #endregion

        #region public IModifier CurrentModifier { get; set; }
        public IModifier CurrentModifier
        {
            get => _CurrentDelta;
            set
            {
                if (_CurrentDelta != null)
                {
                    _CurrentDelta.RemoveTerminateDependent(this);
                    _CurrentDelta.RemoveChangeMonitor(this);
                }

                if (value != null)
                {
                    _CurrentDelta = value;
                    _CurrentDelta.AddTerminateDependent(this);
                    _CurrentDelta.AddChangeMonitor(this);
                    DoValueChanged();
                }
            }
        }
        #endregion

        #region IDelta Members
        public int Value => _CurrentDelta.Value;
        public object Source => _CurrentDelta.Source;
        public string Name => _CurrentDelta.Name;
        public bool Enabled { get { return _CurrentDelta.Enabled; } set { _CurrentDelta.Enabled = value; } }
        #endregion

        #region IControlValue<DeltaValue> Members
        protected void DoValueChanged()
        {
            _ChangeCtrlr.DoValueChanged(new DeltaValue(Value));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Value"));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Name"));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Source"));
            }
        }

        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
            => _ChangeCtrlr.AddChangeMonitor(subscriber);

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
            => _ChangeCtrlr.RemoveChangeMonitor(subscriber);

        #endregion

        #region IControlTerminate Members
        public void DoTerminate()
        {
            // kill the dependents
            _Terminator.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Terminator.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Terminator.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion

        #region IDependOnTerminate Members
        public void Terminate(object sender)
        {
            // if the pointer terminates, so do we
            DoTerminate();
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            DoValueChanged();
        }

        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    }
}
