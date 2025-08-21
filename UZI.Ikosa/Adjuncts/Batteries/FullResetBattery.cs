using System;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>All charges reset on specific intervals</summary>
    [Serializable]
    public class FullResetBattery : PowerBattery, IFullResetBattery
    {
        #region construction
        public FullResetBattery(object source, int charges, double rechargeTime, double nextRecharge)
            : base(source, charges)
        {
            _RechargeTime = rechargeTime;
            _NextRecharge = nextRecharge;
        }
        #endregion

        #region data
        private double _RechargeTime;
        private double _NextRecharge;
        #endregion

        public override bool IsProtected
            => true;

        public override string CapacityDescription
        {
            get
            {
                var (_number, _timeUnit) = TimeConverter.GetBestExpression(RechargeTime);
                return $@"{base.CapacityDescription}, per {_number} {_timeUnit.ValueName(_number)}";
            }
        }

        public double RechargeTime => _RechargeTime;
        public double NextRecharge { get => _NextRecharge; set => _NextRecharge = value; }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextRecharge) && (direction == TimeValTransition.Entering))
            {
                // cyclic, not offset
                while (_NextRecharge < timeVal)
                {
                    _NextRecharge += _RechargeTime;
                }

                AddCharges(UsedCharges);
            }
        }

        public double Resolution
            => Minute.UnitFactor;

        #endregion

        public override object Clone()
            => new FullResetBattery(Source, MaximumCharges.BaseValue, RechargeTime, NextRecharge);
    }
}
