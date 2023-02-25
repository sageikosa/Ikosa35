using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class RequireSuccessCheckFeedback : PrerequisiteFeedback
    {
        public RequireSuccessCheckFeedback(object source, SuccessCheckPrerequisite checkPre)
            : base(source)
        {
            _CheckPre = checkPre;
        }

        private SuccessCheckPrerequisite _CheckPre;
        public SuccessCheckPrerequisite Prequisite => _CheckPre;

        public override IEnumerable<StepPrerequisite> Prerequisites
            => _CheckPre.ToEnumerable();
    }
}
