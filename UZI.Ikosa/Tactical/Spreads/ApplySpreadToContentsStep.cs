using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ApplySpreadToContentsStep : CoreStep
    {
        public ApplySpreadToContentsStep(CoreProcess process)
            : base(process)
        {
        }

        public override bool IsDispensingPrerequisites => throw new NotImplementedException();

        protected override bool OnDoStep()
        {
            throw new NotImplementedException();
        }

        protected override StepPrerequisite OnNextPrerequisite()
        {
            throw new NotImplementedException();
        }
    }
}
