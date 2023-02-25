using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Used by LocalTurnStep to ask an actor whether they will make an action at this time
    /// </summary>
    [Serializable]
    public class ActionInquiryPrerequisite : StepPrerequisite
    {
        public ActionInquiryPrerequisite(CoreStep step, LocalActionBudget budget)
            :base(step, budget.Actor, null, null, @"Turn.WillAct", @"Act Now?")
        {
            _Budget = budget;
            _Acted = null;
        }

        #region private data
        private bool? _Acted;
        private LocalActionBudget _Budget;
        #endregion

        public override bool IsSerial { get { return true; } }
        public override bool IsReady { get { return _Acted.HasValue; } }
        public LocalActionBudget Budget { get { return _Budget; } }

        public bool? Acted { get { return _Acted; } set { _Acted = value; } }

        public override bool FailsProcess { get { return false; } }

        public override CoreActor Fulfiller
        {
            get { return Qualification.Actor; }
        }

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            return ToInfo<ActionInquiryPrerequisiteInfo>(step);
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            // NOTE: do not merge into from client
        }
    }
}
