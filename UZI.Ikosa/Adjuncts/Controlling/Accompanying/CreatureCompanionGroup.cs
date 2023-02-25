using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class CreatureCompanionGroup : ActorControlGroup
    {
        public CreatureCompanionGroup(object source) 
            : base(source)
        {
        }

        public override void ValidateGroup()
        {
        }

        public CreatureMaster CreatureMaster => ActorController as CreatureMaster;
        public CreatureCompanion CreatureCompanion => ActorControlled as CreatureCompanion;
    }
}
