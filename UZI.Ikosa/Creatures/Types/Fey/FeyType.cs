using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class FeyType : CreatureType
    {
        public override string Name => @"Fey";
        public override bool IsLiving => true;
    }
}
