using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class RoundMarkerCompletePrerequisite : StepPrerequisite
    {
        #region ctor()
        public RoundMarkerCompletePrerequisite(LocalTurnTracker tracker)
            : base(tracker, @"RoundMarker.TickComplete", @"Non affiliated actors are done acting")
        {
            _Push = false;
        }
        #endregion

        #region data
        private bool _Push;
        #endregion

        public override bool FailsProcess => false;
        public override CoreActor Fulfiller => null;
        public LocalTurnTracker Tracker => Source as LocalTurnTracker;

        /// <summary>Pushes the prerequisite into a ready state</summary>
        public void PushForward()
        {
            _Push = true;
        }

        public override bool IsReady => _Push;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => null;

        public override void MergeFrom(PrerequisiteInfo info)
        {
            // nothing to do
        }
    }
}
