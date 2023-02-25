using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DisabledEffect : Adjunct, IMonitorChange<CoreActivity>, ITrackTime
    {
        /*
        0 hit points, or negative hit points but stable and conscious
         * single move action or standard action each round (but not both, nor any full-round actions). 
         * move at half speed. 
        move action doesn’t risk further injury, 
         * but any standard action (or any strenuous activity, including casting a quickened spell) deals 1 point of damage after the completion of the act. 
         * Unless the action increased the disabled character’s hit points, she is now in negative hit points and dying. 
        negative hit points recovers hit points naturally if being helped. 
         * Otherwise, each day 10% chance to start recovering hit points naturally (starting with that day); 
         *                 otherwise, loses 1 hit point. 
        Once recovering hit points naturally, no longer losing hit points (even if hit points are negative). 
         */
        public DisabledEffect(object source)
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

        #region IMonitorChange<CoreActivity> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args)
        {
            throw new NotImplementedException();
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            throw new NotImplementedException();
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITrackTime Members
        public void TrackTime(double timeVal)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public double Resolution
        {
            get { return Day.UnitFactor; }
        }
        #endregion

        public override object Clone()
        {
            return new DisabledEffect(Source);
        }
    }
}
