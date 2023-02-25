using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class CohortGroup : ActorControlGroup
    {
        public CohortGroup(object source) 
            : base(source)
        {
        }

        public override void ValidateGroup()
        {
        }

        public SeniorCohort SeniorCohort => ActorController as SeniorCohort;
        public Cohort Cohort => ActorControlled as Cohort;
    }
}
