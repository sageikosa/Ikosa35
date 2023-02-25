using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicFamiliarGroup : ActorControlGroup
    {
        public MagicFamiliarGroup(object source)
            : base(source)
        {
        }

        public override void ValidateGroup()
        {
        }

        public MagicFamiliarCaster MagicFamiliarCaster => ActorController as MagicFamiliarCaster;
        public MagicFamiliar MagicFamiliar => ActorControlled as MagicFamiliar;
    }
}
