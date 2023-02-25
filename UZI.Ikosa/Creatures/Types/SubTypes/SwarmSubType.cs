using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class SwarmSubType : CreatureSubType
    {
        public SwarmSubType(object source)
            : base(source)
        {
        }

        public override string Name => @"Swarm";

        public override CreatureSubType Clone(object source)
            => new SwarmSubType(Source);
    }
}
