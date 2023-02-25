using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class Lodging : GroupMasterAdjunct
    {
        public Lodging(LodgingGroup lodging)
            : base(lodging, lodging)
        {
        }

        public override object Clone()
            => new Lodging(LodgingGroup);

        public LodgingGroup LodgingGroup => Group as LodgingGroup;
    }
}
