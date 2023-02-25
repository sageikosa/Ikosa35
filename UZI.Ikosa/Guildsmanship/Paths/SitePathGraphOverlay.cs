using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class SitePathGraphOverlay : ModuleLink<SitePathGraph>
    {
        public SitePathGraphOverlay(Description description) 
            : base(description)
        {
        }
    }
}
