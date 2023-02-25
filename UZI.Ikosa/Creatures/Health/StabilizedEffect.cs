using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    [Serializable]
    public class StabilizedEffect : Adjunct, ITrackTime
    {
        /*
        Dying but stopped losing hit points and still has negative hit points is stable. 
         * The character is no longer dying, but is still unconscious. 
        If becomes stable because of aid (Heal check or magical healing), 
         *   no longer loses hit points. 
         *   10% chance each hour of becoming conscious and disabled (hit points are still negative). 
        stable on own without help, 
         *   Each hour, 10% chance of becoming conscious and disabled. Otherwise loses 1 hit point. 
         */

        public StabilizedEffect(object source)
            : base(source)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public double Resolution
        {
            get { return Hour.UnitFactor; }
        }
        #endregion

        public override object Clone()
        {
            return new StabilizedEffect(Source);
        }
    }
}
