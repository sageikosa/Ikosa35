using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class AttackTriggerablePostTriggerStep : CoreStep
    {
        public AttackTriggerablePostTriggerStep(CoreProcess process, AttackTriggerable triggerable) 
            : base(process)
        {
            _AtkTriggerable = triggerable;
        }

        #region data
        private AttackTriggerable _AtkTriggerable;
        #endregion

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            _AtkTriggerable.DoPostTrigger();
            return true;
        }
    }
}
