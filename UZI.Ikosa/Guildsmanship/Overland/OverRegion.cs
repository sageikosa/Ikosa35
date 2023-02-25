using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class OverRegion : ModuleLink<Region>
    {
        public OverRegion(Description description)
            : base(description)
        {
        }
    }
}
