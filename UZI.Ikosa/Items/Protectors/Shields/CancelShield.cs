using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Shields
{
    [Serializable]
    public class CancelShield : Adjunct, ITrackTime
    {
        public CancelShield(ShieldBase shield, double endTime) 
            : base(shield)
        {
            _EndTime = endTime;
        }

        private double _EndTime;

        public ShieldBase Shield => Source as ShieldBase;
        public double EndTime => _EndTime;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Shield.Enabled = false;
        }

        protected override void OnDeactivate(object source)
        {
            Shield.Enabled = true;
            base.OnDeactivate(source);
        }

        public double Resolution => Round.UnitFactor;

        public override object Clone()
            => new CancelShield(Shield, EndTime);

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= EndTime) && (direction == TimeValTransition.Entering))
            {
                Eject();
            }
        }
    }
}
