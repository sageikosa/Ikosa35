using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RaySurgeFromTransitionInfo : RaySurgeTransitionInfo
    {
        public override TransientVisualizer GetTransientVisualizer()
        {
            return GetRayTransition<RaySurgeFromTransition>();
        }
    }
}
