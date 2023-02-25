using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class NeedsTurnTickPrerequisite : StepPrerequisite
    {
        public NeedsTurnTickPrerequisite(LocalTurnTracker tracker)
            : base(tracker, @"LocalTurnTracker.NeedsTurnTick", @"Anything needing a turn tick needs to be added now")
        {
        }

        public override bool FailsProcess => false;
        public override CoreActor Fulfiller => null;
        public LocalTurnTracker Tracker => Source as LocalTurnTracker;

        /// <summary>No creature is allowed to still have needs turn tick after this step</summary>
        public override bool IsReady
            => !Tracker.ContextSet.GetCoreIndex()
            .Select(_idx => _idx.Value)
            .OfType<Creature>()
            .Any(_c => _c.HasAdjunct<NeedsTurnTick>());

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
            => null;

        public override void MergeFrom(PrerequisiteInfo info)
        {
            // nothing to do
        }
    }
}
