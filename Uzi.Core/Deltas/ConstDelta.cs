using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>
    /// Value changes are ignored.  Cannot use source-terminate, must be removed from where it is used.
    /// Does not respond to IMonitorChange interfaces or IDependOnTerminate interfaces.
    /// Do not use if delta can be disabled.
    /// Excellent as IDelta to transient deltable
    /// </summary>
    [Serializable]
    public class ConstDelta : IModifier
    {
        #region Constructor
        public ConstDelta(int modifier, object source)
        {
            _Value = modifier;
            _Src = source;
            _Name = Source.SourceName();
            _TCtrl = new TerminateController(this);
        }

        public ConstDelta(int modifier, object source, string name)
        {
            _Value = modifier;
            _Src = source;
            _Name = name;
            _TCtrl = new TerminateController(this);
        }
        #endregion

        #region data
        protected int _Value;
        protected object _Src;
        protected string _Name;
        private readonly TerminateController _TCtrl;
        #endregion

        #region IDelta Members
        #region public int Value {get;set;}
        public int Value
        {
            get
            {
                return _Value;
            }
            set
            {
                // no change allowed!
            }
        }
        #endregion

        public object Source => _Src;
        public string Name => _Name;

        #region public bool Enabled { get; set; }
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
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all deltables using this delta to release it.  
        /// Note: this does not destroy the delta and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _TCtrl?.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl?.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl?.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _TCtrl?.TerminateSubscriberCount ?? 0;
        #endregion
        #endregion

        #region IControlValue<DeltaValue> Members
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber) { }
        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber) { }
        #endregion

        public override string ToString() => Delta.FormatModifier(_Value, _Name);

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
