using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicFamiliar : ActorControlled<MagicFamiliarGroup>
    {
        public MagicFamiliar(object source, MagicFamiliarGroup familiarGroup)
            : base(source, familiarGroup)
        {
        }

        public override object Clone()
        {
            return new MagicFamiliar(Source, ControlGroup);
        }
    }
}
