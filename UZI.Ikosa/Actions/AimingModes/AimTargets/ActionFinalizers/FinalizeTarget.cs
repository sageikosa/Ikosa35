using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class FinalizeTarget : AimTarget
    {
        protected FinalizeTarget(IInteract target)
            : base(@"~", target)
        {
        }

        public abstract void DoActionFinalized(CoreActivity activity, bool deactivated);
        public abstract bool SerialStateDependent { get; }
    }
}
