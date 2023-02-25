using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ExtendTickStep : CoreStep, ITurnTrackingStep
    {
        public ExtendTickStep(CoreActivity activity, TimeTickablePrerequisite timeTickPre, int duration)
            : base(activity)
        {
            _PreReq = timeTickPre;
            _Duration = duration;
        }

        #region data
        private TimeTickablePrerequisite _PreReq;
        private int _Duration;
        #endregion

        public LocalTurnTracker Tracker => _PreReq.Tracker;

        public override bool IsDispensingPrerequisites
            => false;

        protected override bool OnDoStep()
        {
            _PreReq.ExtendTick((Process as CoreActivity)?.Actor as Creature, _Duration);
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;
    }
}
