using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RayBoltTransitionInfo : RayTransitionInfo
    {
        [DataMember]
        public double Length { get; set; }

        public override TransientVisualizer GetTransientVisualizer()
        {
            var _bolt = GetRayTransition<RayBoltTransition>();
            _bolt.Length = Length;
            return _bolt;
        }

    }
}
