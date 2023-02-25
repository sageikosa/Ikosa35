using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class RoadPath : SitePath
    {
        public RoadPath(Description description, Guid source, Guid target) 
            : base(description, source, target, 1m)
        {
            LosePathChance = 10m;
        }

        public RoadPath(Description description, Guid source, Guid target, decimal reverseFactor) 
            : base(description, source, target, reverseFactor)
        {
            LosePathChance = 10m;
        }
    }
}
