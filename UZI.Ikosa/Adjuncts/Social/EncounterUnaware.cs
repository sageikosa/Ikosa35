using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EncounterUnaware : GroupParticipantAdjunct
    {
        public EncounterUnaware(object source, EncounterGroup group) 
            : base(source, group)
        {
        }

        public EncounterGroup EncounterGroup
            => Group as EncounterGroup;

        public override object Clone()
            => new EncounterUnaware(Source, EncounterGroup);
    }
}
