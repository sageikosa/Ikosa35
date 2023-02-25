using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PowerBattery : Adjunct, IPowerBattery, IProtectable, IMonitorChange<DeltaValue>
    {
        #region construction
        public PowerBattery(object source, int charges)
            : base(source)
        {
            _Used = 0;
            _MaxCharges = new Deltable(charges);
            _CCtrl = new ChangeController<int>(this, charges);
        }
        #endregion

        #region data
        private IDeltable _MaxCharges;
        private int _Used;
        private ChangeController<int> _CCtrl;
        #endregion

        // IPowerCapacity
        public virtual string CapacityDescription
            => $@"{AvailableCharges} uses";

        public virtual bool CanUseCharges(int number)
            => AvailableCharges >= number;

        public virtual void AddCharges(int number)
        {
            _Used -= number;
            if (_Used < 0)
                _Used = 0;
            DoChargeChange();
        }

        // IPowerBattery 
        public IDeltable MaximumCharges => _MaxCharges;
        public virtual int UsedCharges => _Used;

        public virtual void UseCharges(int number) { _Used += number; DoChargeChange(); }

        public virtual int AvailableCharges
            => MaximumCharges.QualifiedValue(null) - UsedCharges;
        
        // IProtectable
        public bool IsExposedTo(Creature critter)
            => this.HasExposureTo(critter);

        public override object Clone()
            => new PowerBattery(Source, MaximumCharges.BaseValue);

        #region IControlChange<int> Members
        protected void DoChargeChange()
        {
            _CCtrl.DoValueChanged(MaximumCharges.QualifiedValue(null) - _Used);
            DoPropertyChanged(@"AvailableCharges");
        }

        public void AddChangeMonitor(IMonitorChange<int> monitor)
        {
            _CCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<int> monitor)
        {
            _CCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region IMonitorChange<DeltaValue> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            DoChargeChange();
        }

        #endregion
    }

    public static class PowerBatteryHelpers
    {
        /// <summary>Ensures the magic power effect is tracking a power battery, and returns that battery</summary>
        public static PowerBattery EnsureBattery(this MagicPowerEffect magicPowerEffect, Func<int> initialCharges)
        {
            var _battery = magicPowerEffect.GetTargetValue<PowerBattery>(@"Battery");
            if (_battery == null)
            {
                _battery = new PowerBattery(magicPowerEffect, initialCharges());
                var _target = new ValueTarget<PowerBattery>(@"Battery", _battery);
                magicPowerEffect.AllTargets.Add(_target);
            }
            return _battery;
        }
    }
}
