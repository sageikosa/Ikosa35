using System;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class TimeTickablePrerequisite : StepPrerequisite
    {
        #region ctor()
        public TimeTickablePrerequisite(LocalTurnTracker tracker)
            : base(tracker, @"Time.Tickable", @"Time can tick forward one round")
        {
            _LastAct = DateTime.Now;
            _RealEnd = _LastAct.AddSeconds(6);
            _NextTime = _RealEnd;
            _MaxTime = _NextTime.AddSeconds(9);
            _Push = Tracker.CanTimelineFlow;
        }
        #endregion

        #region data
        private DateTime _RealEnd;
        private DateTime _LastAct;
        private DateTime _NextTime;
        private DateTime _MaxTime;
        private bool _Push;
        #endregion

        public DateTime RealEndTime => _RealEnd;
        public DateTime LastActTime => _LastAct;
        public DateTime NextEndTime => _NextTime;
        public override bool FailsProcess => false;
        public override CoreActor Fulfiller => null;
        public LocalTurnTracker Tracker => Source as LocalTurnTracker;

        /// <summary>Pushes the prerequisite into a ready state ahead of its time expiration</summary>
        public void PushForward()
        {
            _Push = true;
        }

        /// <summary>
        /// ExtendTick by the greater of the absolute increase to scheduled duration
        /// or the relative increase to current time.
        /// </summary>
        public void ExtendTick(Creature creature, int absMilliSeconds)
        {
            _LastAct = DateTime.Now;

            // _absNew will always be beyond current scheduled _NextTime
            var _absNew = _NextTime.AddMilliseconds(absMilliSeconds);
            if (_absNew <= _MaxTime)
            {
                _NextTime = _absNew;
            }
        }

        /// <summary>Pushed, or time can tick and Now is the time</summary>
        public override bool IsReady
            => _Push
            || (Tracker.IsAutoTimeTick
                && ((DateTime.Now >= _NextTime)
                || ((DateTime.Now >= _RealEnd) && (DateTime.Now - _LastAct).TotalMilliseconds > 1000)));

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => null;

        public override void MergeFrom(PrerequisiteInfo info)
        {
            // nothing to do
        }
    }
}
