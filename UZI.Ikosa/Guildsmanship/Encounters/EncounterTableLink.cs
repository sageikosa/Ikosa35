using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class EncounterTableLink : ModuleLink<EncounterTable>
    {
        public EncounterTableLink(Description description) 
            : base(description)
        {
        }
    }
}
