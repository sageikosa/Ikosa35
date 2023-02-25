using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PromptTurnTrackerStep : PreReqListStepBase, ITurnTrackingStep
    {
        // TODO: prompt from timeline?

        public PromptTurnTrackerStep(ITurnTrackingStep timeStep, IList<Creature> awarenessTriggered)
            : base(timeStep as CoreStep)
        {
            _Triggered = awarenessTriggered;
            _PendingPreRequisites.Enqueue(new PromptTurnTrackerPrerequisite(this));
            _Tracker = timeStep.Tracker;
        }

        #region data
        private LocalTurnTracker _Tracker;
        private IList<Creature> _Triggered;
        #endregion

        public LocalTurnTracker Tracker => _Tracker;

        public IList<Creature> AwarenessTriggered => _Triggered;

        public PromptTurnTrackerPrerequisite PromptTurnTrackerPrerequisite 
            => DispensedPrerequisites.OfType<PromptTurnTrackerPrerequisite>().FirstOrDefault();

        protected override bool OnDoStep()
            => true;
    }

    [Serializable]
    public class PromptTurnTrackerPrerequisite : StepPrerequisite
    {
        public PromptTurnTrackerPrerequisite(PromptTurnTrackerStep step)
            : base(step, @"PromptTurnTrackerPrerequisite", @"Setup Turn Tracker?")
        {
            _Done = false;
        }

        #region data
        private bool _Done;
        #endregion

        public PromptTurnTrackerStep PromptTurnTrackerStep => Source as PromptTurnTrackerStep;
        public bool Done { get { return _Done; } set { _Done = value; } }

        /// <summary>Only the game master can handle this</summary>
        public override CoreActor Fulfiller => null;

        public override bool FailsProcess => false;

        public override bool IsReady => _Done;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<PromptTurnTrackerPrerequisiteInfo>(step);
            _info.Triggered = PromptTurnTrackerStep.AwarenessTriggered.Select(_c => _c.ID).ToList();
            _info.Done = Done;
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            Done = (info as PromptTurnTrackerPrerequisiteInfo)?.Done ?? Done;
        }
    }
}
