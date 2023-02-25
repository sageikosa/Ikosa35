using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class WrapperBattery : PowerBattery, IMonitorChange<int>
    {
        public WrapperBattery(object source, int charges, PowerBattery wrapped)
            : base(source, charges)
        {
            _Wrapped = wrapped;
        }

        private PowerBattery _Wrapped;
        public PowerBattery WrappedBattery { get { return _Wrapped; } }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (WrappedBattery != null)
            {
                WrappedBattery.AddChangeMonitor(this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (WrappedBattery != null)
            {
                WrappedBattery.RemoveChangeMonitor(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override void UseCharges(int number)
        {
            WrappedBattery.UseCharges(number);
            base.UseCharges(number);
        }

        public override int AvailableCharges
        {
            get
            {
                // use the lesser of our battery charges and the wrapped battery charges
                if (base.AvailableCharges > WrappedBattery.AvailableCharges)
                {
                    return WrappedBattery.AvailableCharges;
                }
                else
                {
                    return base.AvailableCharges;
                }
            }
        }

        #region IMonitorChange<int> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            // if fully reset, the power is available again
            if ((WrappedBattery != null) && (WrappedBattery.UsedCharges == 0) && (UsedCharges > 0))
            {
                AddCharges(UsedCharges);
            }
        }

        #endregion
    }
}
