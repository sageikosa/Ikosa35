using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class ExtraplanarSubType : CreatureSubType
    {
        public ExtraplanarSubType(object source)
            : base(source)
        {
        }

        public override string Name => @"Extraplanar";

        public override CreatureSubType Clone(object source)
            => new ExtraplanarSubType(Source);
    }
}
