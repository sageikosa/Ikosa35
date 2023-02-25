using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class DragonType : CreatureType
    {
        public override string Name => @"Dragon";
        public override bool IsLiving => true;
    }
}
