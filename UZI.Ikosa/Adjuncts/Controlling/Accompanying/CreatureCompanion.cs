using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class CreatureCompanion : ActorControlled<CreatureCompanionGroup>
    {
        public CreatureCompanion(object source, CreatureCompanionGroup companionGroup) 
            : base(source, companionGroup)
        {
        }

        public override object Clone()
        {
            return new CreatureCompanion(Source, ControlGroup);
        }
    }
}
