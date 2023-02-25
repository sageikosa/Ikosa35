using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Cohort : ActorControlled<CohortGroup>
    {
        public Cohort(object source, CohortGroup cohortGroup)
            : base(source, cohortGroup)
        {
        }

        public override object Clone()
        {
            return new Cohort(Source, ControlGroup);
        }
    }
}
