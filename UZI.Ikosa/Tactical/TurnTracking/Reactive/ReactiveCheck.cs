using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ReactiveCheck : PreReqListStepBase
    {
        public ReactiveCheck(Info trigger, CoreActor actor, IEnumerable<(IActionProvider, ActionBase)> actions)
            : base((CoreProcess)null)
        {
            _PendingPreRequisites.Enqueue(new ReactivePrerequisite(trigger, actor, actions));
        }

        protected override bool OnDoStep()
        {
            // NOTHING TO DO?
            return true;
        }
    }
}
