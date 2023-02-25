using System;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class AwarenessTarget : AimTarget
    {
        #region Construction
        public AwarenessTarget(string key, IInteract target, SegmentSet planLine)
            : base(key, target)
        {
            PlanLine = planLine;
        }
        #endregion

        public SegmentSet PlanLine { get; private set; }

        public override AimTargetInfo GetTargetInfo()
        {
            var _info = ToInfo<AwarenessTargetInfo>();
            return _info;
        }
    }
}
