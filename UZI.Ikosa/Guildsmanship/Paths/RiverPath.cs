using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class RiverPath : SitePath
    {
        public RiverPath(Description description, Guid source, Guid target, decimal reverseFactor) 
            : base(description, source, target, reverseFactor)
        {
            LosePathChance = 0m;
        }
    }
}
