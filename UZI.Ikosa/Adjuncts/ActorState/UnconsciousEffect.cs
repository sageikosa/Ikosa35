using System;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Filters actions and adds the unconscious condition for a duration</summary>
    [Serializable]
    public class UnconsciousEffect : SleepEffect, ITrackTime
    {
        #region Construction
        public UnconsciousEffect(object source, double endTime, double resolution)
            : base(source, false)
        {
            _EndTime = endTime;
            _TimeRes = resolution;
        }
        #endregion

        protected override void OnActivate(object source)
        {
            Critter?.Conditions.Add(new Condition(Condition.Unconscious, this));
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.Unconscious, this]);
            }
            base.OnDeactivate(source);
        }

        public override void Awaken()
        {
            // not possible?
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _EndTime) && (direction == TimeValTransition.Entering))
            {
                Anchor.RemoveAdjunct(this);
            }
        }

        private double _EndTime;
        private double _TimeRes;
        public double Resolution { get { return _TimeRes; } }
        #endregion
    }
}
