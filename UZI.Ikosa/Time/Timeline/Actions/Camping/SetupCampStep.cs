using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class SetupCampStep : CoreStep
    {
        public SetupCampStep(CoreProcess process)
            : base(process)
        {
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites => false;

        protected override bool OnDoStep()
        {
            if (Process is CoreActivity _activity)
            {
                // setup member adjunct
                var _campingGroup = _activity.GetFirstTarget<ValueTarget<CampingGroup>>(nameof(CampingGroup))?.Value;
                _activity.Actor.AddAdjunct(new CampMember(_campingGroup));

                // TODO: get out camping gear...? bind with group adjunct
            }
            return true;
        }
    }
}
