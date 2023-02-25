using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SeniorCohort : ActorController<CohortGroup>
    {
        public SeniorCohort(object source, CohortGroup controlGroup) 
            : base(source, controlGroup)
        {
        }

        public override object Clone()
        {
            return new SeniorCohort(Source, ControlGroup);
        }
    }
}
