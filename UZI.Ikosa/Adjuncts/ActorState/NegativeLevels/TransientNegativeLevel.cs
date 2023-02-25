using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{

    [Serializable]
    public class TransientNegativeLevel : NegativeLevel, ITrackTime
    {
        public TransientNegativeLevel(object source, double endTime)
            : base(source)
        {
            _EndTime = endTime;
        }

        #region data
        private double _EndTime;
        #endregion

        public double EndTime => _EndTime;

        public override object Clone()
            => new TransientNegativeLevel(Source, EndTime);

        public double Resolution => Hour.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= EndTime)
            {
                // automatically ejects when time's up
                Eject();
            }
        }
    }
}
