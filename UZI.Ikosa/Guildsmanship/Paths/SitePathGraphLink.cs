using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class SitePathGraphLink : ModuleLink<SitePathGraph>
    {
        public SitePathGraphLink(Description description)
            : base(description)
        {
        }
    }
}
