using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EncounterGroup : AdjunctGroup
    {
        public EncounterGroup()
            : base(typeof(EncounterGroup))
        {
        }

        public IEnumerable<EncounterAware> EncounterAwares => Members.OfType<EncounterAware>();
        public IEnumerable<EncounterUnaware> EncounterUnawares => Members.OfType<EncounterUnaware>();
        public override void ValidateGroup() { }
    }
}
