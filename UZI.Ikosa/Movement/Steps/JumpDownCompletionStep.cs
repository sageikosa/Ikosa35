using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class JumpDownCompletionStep : MovementProcessStep
    {
        public JumpDownCompletionStep(CoreActivity activity) 
            : base(activity)
        {
        }

        public override bool IsDispensingPrerequisites
            => false;

        protected override bool OnDoStep()
        {
            // cleanup
            MovementAction?.Creature?
                .Adjuncts.OfType<JumpingDown>().FirstOrDefault()?
                .Eject();
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;
    }
}
