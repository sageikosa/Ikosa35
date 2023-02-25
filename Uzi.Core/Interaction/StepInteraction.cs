using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class StepInteraction : Interaction
    {
        public StepInteraction(CoreStep step, CoreActor actor, object source, IInteract target,
            InteractData interactData, bool useDefault = true)
            : base(actor, source, target, interactData, useDefault)
        {
            _Step = step;
        }

        private readonly CoreStep _Step;
        public CoreStep Step => _Step;

        public void DoTargetNotifyStep(SysNotify notify)
        {
            if (Target != null)
            {
                _Step?.EnqueueNotify(notify, Target.ID);
            }
        }
    }
}
