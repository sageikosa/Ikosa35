using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using Uzi.Core.Contracts;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Uzi.Core
{
    [Serializable]
    public class Deltable : IEnumerable<IDelta>, IMonitorChange<DeltaValue>, IDeltable
    {
        #region Constructor
        public Deltable(int seedValue)
        {
            _BVal = seedValue;
            _DSet = new DeltaSet(this);
            _CCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(seedValue));
            Deltas.AddChangeMonitor(this);
        }
        #endregion

        #region data
        private double _BVal;
        private int _LastVal = 0;
        private readonly DeltaSet _DSet;
        private readonly ChangeController<DeltaValue> _CCtrl;
        #endregion

        #region public virtual int BaseValue { get; set; }
        public virtual int BaseValue
        {
            get => (int)Math.Floor(_BVal);
            set
            {
                _BVal = value;
                DoValueChanged();
            }
        }
        #endregion

        #region public virtual double BaseDoubleValue { get; set; }
        public virtual double BaseDoubleValue
        {
            get => _BVal;
            set
            {
                _BVal = value;
                DoValueChanged();
            }
        }
        #endregion

        #region public virtual int EffectiveValue {get;}
        /// <summary>Unqualified effective value</summary>
        public int EffectiveValue
        {
            get
            {
                _LastVal = CalcEffectiveValue();
                return _LastVal;
            }
        }

        /// <summary>
        /// Override to change calculation mechanism.  Accounts for tracking true changes.
        /// </summary>
        /// <returns></returns>
        protected virtual int CalcEffectiveValue()
        {
            return BaseValue + Deltas.Value;
        }
        #endregion

        /// <summary>Effective value for a condition</summary>
        public virtual int QualifiedValue(Qualifier qualification, DeltaCalcInfo calcInfo = null)
        {
            var _log = calcInfo != null;
            if (_log)
            {
                calcInfo.BaseValue = BaseValue;
            }
            var _result = BaseValue + Deltas.QualifiedValue(qualification, calcInfo);
            if (_log)
            {
                calcInfo.Result = _result;
            }
            return _result;
        }

        /// <summary>Used to support calculations (such as die rolls)</summary>
        public virtual IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify, object baseSource, string baseName)
        {
            if (BaseValue != 0)
            {
                yield return new QualifyingDelta(BaseValue, baseSource, baseName);
            }
            foreach (var _del in Deltas.GetQualifiedDeltas(qualify))
            {
                yield return _del;
            }
            yield break;
        }

        /// <summary>Set of IDeltas</summary>
        public DeltaSet Deltas => _DSet;

        #region ValueChanged Event
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));

        protected virtual void DoValueChanged()
        {
            _CCtrl.DoValueChanged(new DeltaValue(EffectiveValue));
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(EffectiveValue)));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BaseValue)));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BaseDoubleValue)));
            }
        }

        #region IControlValue<DeltaValue> Members
        /// <summary>Add Change Monitor for DeltaValue</summary>
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _CCtrl.AddChangeMonitor(subscriber);
        }

        /// <summary>Remove Change Monitor for DeltaValue</summary>
        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _CCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        #region IEnumerable<IDelta> Members
        public virtual IEnumerator<IDelta> GetEnumerator()
        {
            foreach (IDelta _mod in Deltas)
            {
                yield return _mod;
            }
            yield break;
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            DeltableValueChanged();
        }

        /// <summary>
        /// Allows extension to IMonitorChange&lt;DeltaValue&gt;.ValueChanged
        /// </summary>
        protected void DeltableValueChanged()
        {
            // prevent non-changing modifications from cascading
            var _lVal = _LastVal;
            if (_lVal != EffectiveValue)
                DoValueChanged();
        }

        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }
        #endregion

        public Collection<string> DeltaDescriptions
            => new Collection<string>(Deltas.Select(_del => Delta.FormatModifier(_del)).ToList());

        protected DInfo ToInfo<DInfo>(Qualifier qualifier)
            where DInfo : DeltableInfo, new()
        {
            return new DInfo
            {
                BaseDoubleValue = BaseDoubleValue,
                BaseValue = BaseValue,
                EffectiveValue = QualifiedValue(qualifier),
                DeltaDescriptions = DeltaDescriptions
            };
        }

        public DeltableInfo ToDeltableInfo(Qualifier qualifier = null)
            => ToInfo<DeltableInfo>(qualifier);

        public VolatileValueInfo ToVolatileValueInfo(Qualifier qualifier = null)
            => ToDeltableInfo(qualifier);

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [field:NonSerialized, JsonIgnore]
        public static Func<Guid, string, DeltaCalcNotify> CreateDeltaCalcNotify { get; set; }

        [field:NonSerialized, JsonIgnore]
        public static Func<Guid, string, Guid, string, CheckNotify> CreateCheckNotify { get; set; }

        public static DeltaCalcNotify GetDeltaCalcNotify(Guid? id, string description)
            => CreateDeltaCalcNotify?.Invoke(id ?? Guid.Empty, description) ?? new DeltaCalcNotify(id ?? Guid.Empty, description);

        public static CheckNotify GetCheckNotify(
            Guid? checkPrincipal, string checkTitle,
            Guid? opposedPrincipal, string opposedTitle)
            => CreateCheckNotify?.Invoke(checkPrincipal ?? Guid.Empty, checkTitle, opposedPrincipal ?? Guid.Empty, opposedTitle)
            ?? new CheckNotify(checkPrincipal ?? Guid.Empty, checkTitle, opposedPrincipal ?? Guid.Empty, opposedTitle);
    }
}
