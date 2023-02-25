using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>
    /// Delta
    /// </summary>
    [Serializable]
    public class Delta : IModifier
    {
        #region ctor()
        public Delta(int modifier, object source)
        {
            _TCtrl = new TerminateController(this);
            _Val = modifier;
            _Src = source;
            _Name = source.SourceName();
            _On = true;
        }

        public Delta(int modifier, object source, string name)
        {
            _TCtrl = new TerminateController(this);
            _Val = modifier;
            _Src = source;
            _On = true;
            _Name = name;
        }
        #endregion

        #region private data
        protected object _Src;
        protected string _Name;
        protected int _Val;
        protected bool _On;
        private ChangeController<DeltaValue> _DCtrl = null;
        private TerminateController _TCtrl;
        #endregion

        #region public int Value { get; set; }
        public int Value
        {
            get => _On ? _Val : 0;
            set
            {
                _Val = value;
                DoValueChanged();
            }
        }
        #endregion

        public int EnabledValue { get => _Val; set => Value = value; }

        public object Source => _Src;
        public string Name => _Name;

        #region public bool Enabled { get; set; }
        public bool Enabled
        {
            get => _On;
            set
            {
                _On = value;
                DoValueChanged();
            }
        }
        #endregion

        public static implicit operator int(Delta mod)
            => mod.Value;

        #region Display Formatting
        public static string FormatModifier(IDelta mod)
            => FormatModifier(mod.Value, mod.Name);

        public static string FormatModifier(int val, string name)
            => (val >= 0)
                ? $@"+{val} from {name}"
                : $@"{val} from {name}";

        public override string ToString()
            => FormatModifier(_Val, Name);
        #endregion

        #region ValueChanged Event
        protected void DoValueChanged()
        {
            ValueCtrlr().DoValueChanged(new DeltaValue(Value));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
        }

        #region IControlValue<DeltaValue> Members
        protected ChangeController<DeltaValue> ValueCtrlr()
        {
            if (_DCtrl == null)
            {
                _DCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(_Val));
            }
            return _DCtrl;
        }

        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            ValueCtrlr().AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            ValueCtrlr().RemoveChangeMonitor(subscriber);
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
            _TCtrl.DoTerminate();
        }

        #region IControlTerminate Members
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

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    };
}
