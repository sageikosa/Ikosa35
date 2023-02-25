using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class ActionTimeKeeper : Adjunct, ITrackTime
    {
        public ActionTimeKeeper(object source) 
            : base(source)
        {
        }

        #region state
        private int _Rounds;
        #endregion

        public override object Clone()
            => new ActionTimeKeeper(Source);

        public int Rounds => _Rounds;

        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Entering)
                _Rounds++;
        }

        // TODO: spend kept time for budget use...
    }
}
