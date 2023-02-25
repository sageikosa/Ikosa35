using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class CreatureMaster : ActorController<CreatureCompanionGroup>
    {
        public CreatureMaster(object source, CreatureCompanionGroup controlGroup) 
            : base(source, controlGroup)
        {
        }

        public override object Clone()
        {
            return new CreatureMaster(Source, ControlGroup);
        }
    }
}
