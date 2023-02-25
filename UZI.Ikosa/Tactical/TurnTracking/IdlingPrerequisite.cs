using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    // NOTE: this will eventually be obsolete
    [Serializable]
    public class IdlingPrerequisite : StepPrerequisite
    {
        public IdlingPrerequisite(LocalTurnTracker tracker)
            : base(tracker, @"TimeStep.Idling", @"No pending actions")
        {
        }

        public LocalTurnTracker Tracker
            => Source as LocalTurnTracker;

        public override bool IsReady => true;
        public override bool FailsProcess => false;
        public override CoreActor Fulfiller => null;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => null;

        public override void MergeFrom(PrerequisiteInfo info)
        {
        }
    }
}
