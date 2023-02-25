using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class AberrationType : CreatureType
    {
        public override string Name => @"Aberration";
        public override bool IsLiving => true;
    }
}
