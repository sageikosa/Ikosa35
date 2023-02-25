using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>If the alternate is better than the original, adds the difference between the two</summary>
    [Serializable]
    public class BetterDelta : IModifier, IDependOnTerminate, IMonitorChange<DeltaValue>
    {
        #region construction
        /// <summary>If the alternate is better than the original, adds the difference between the two</summary>
        public BetterDelta(IModifier alternate, IModifier original)
        {
            _Term = new TerminateController(this);
            _CCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(alternate.Value));

            _Alt = alternate;
            _Alt.AddTerminateDependent(this);
            _Alt.AddChangeMonitor(this);

            _Orig = original;
            _Orig.AddTerminateDependent(this);
            _Orig.AddChangeMonitor(this);
        }
        #endregion

        #region data
        private TerminateController _Term;
        private ChangeController<DeltaValue> _CCtrl;
        private IModifier _Alt;
        private IModifier _Orig;
        #endregion

        #region IDelta Members
        public int Value { get { return _Alt.Value > _Orig.Value ? _Alt.Value - _Orig.Value : 0; } }
        public object Source { get { return _Alt.Source; } }
        public string Name { get { return _Alt.Name; } }
        public bool Enabled { get { return _Alt.Enabled; } set { _Alt.Enabled = value; } }
        #endregion

        #region ValueChanged Event
        protected void DoValueChanged()
        {
            _CCtrl.DoValueChanged(new DeltaValue(Value));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Value"));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Name"));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(@"Source"));
            }
        }

        #region IControlValue<DeltaValue> Members

        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _CCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _CCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        public void DoTerminate()
        {
            // kill the dependents
            _Term.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;
        #endregion

        #region IDependOnTerminate Members
        public void Terminate(object sender)
        {
            // if either pointer terminates, so do we
            DoTerminate();
        }
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
