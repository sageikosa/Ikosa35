using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Attach to portals and containers to prevent gases or gaseous creatures from passing.</summary>
    [Serializable]
    public class AirTight : Adjunct
    {
    /// <summary>Attach to portals and containers to prevent gases or gaseous creatures from passing.</summary>
        public AirTight(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new AirTight(Source);
    }
}
