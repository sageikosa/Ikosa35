using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class TrailPath : SitePath
    {
        public TrailPath(Description description, Guid source, Guid target, decimal reverseFactor)
            : base(description, source, target, reverseFactor)
        {
            LosePathChance = 25m;
        }

        public TrailPath(Description description, Guid source, Guid target)
            : base(description, source, target, 1m)
        {
            LosePathChance = 25m;
        }
    }
}
