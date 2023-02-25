using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Senses
{
    public struct AwarenessResult
    {
        public Guid ID { get; set; }
        public AwarenessLevel AwarenessLevel { get; set; }
        public Locator Locator { get; set; }
    }
}
