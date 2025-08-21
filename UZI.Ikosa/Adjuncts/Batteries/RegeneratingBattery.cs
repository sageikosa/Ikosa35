using System;
using System.Collections.Generic;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Each charge regenerates after a certain amount of time has elapsed since its use</summary>
    [Serializable]
    public class RegeneratingBattery : PowerBattery, IRegeneratingBattery
    {
        #region construction
        public RegeneratingBattery(object source, int charges, double rechargeTime)
            : base(source, charges)
        {
            _RechargeTime = rechargeTime;
            _NextRecharge = new Queue<double>();
        }
        #endregion

        #region data
        private Queue<double> _NextRecharge;
        private double _RechargeTime;
        #endregion

        public override string CapacityDescription
        {
            get
            {
                var (_number, _timeUnit) = TimeConverter.GetBestExpression(RechargeTime);
                return $@"{base.CapacityDescription} per {_number} {_timeUnit.ValueName(_number)}";
            }
        }

        public override void UseCharges(int number)
        {
            var _loc = Anchor.GetLocated();
            if (_loc != null)
            {
                // before using these charges, track when they will be usable again
                for (var _nx = 0; _nx < number; _nx++)
                {
                    _NextRecharge.Enqueue(_loc.Locator.Map.CurrentTime + RechargeTime);
                }
            }
            base.UseCharges(number);
        }

        public double RechargeTime => _RechargeTime; 

        #region ITrackTime Members
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // recovers if the recharge time has expired
            if ((direction == TimeValTransition.Entering)
                && (_NextRecharge.Count > 0))
            {
                while (_NextRecharge.Peek() <= timeVal)
                {
                    var _val = _NextRecharge.Dequeue();
                    if (UsedCharges > 0)
                    {
                        AddCharges(1);
                    }
                }
            }
        }

        public double Resolution => Minute.UnitFactor;
        #endregion

        public override object Clone()
        {
            return new RegeneratingBattery(Source, MaximumCharges.BaseValue, RechargeTime);
        }
    }
}
