using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicFamiliarCaster : ActorController<MagicFamiliarGroup>
    {
        public MagicFamiliarCaster(object source, MagicFamiliarGroup controlGroup) 
            : base(source, controlGroup)
        {
        }

        public override object Clone()
        {
            return new MagicFamiliarCaster(Source, ControlGroup);
        }
    }
}
