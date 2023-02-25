using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RayTransitionInfo : LinearTransitionInfo
    {
        [DataMember]
        public double HeadWidth { get; set; }
        [DataMember]
        public double TailWidth { get; set; }

        public override TransientVisualizer GetTransientVisualizer()
        {
            return GetRayTransition<RayTransition>();
        }

        protected RTransition GetRayTransition<RTransition>()
            where RTransition : RayTransition, new()
        {
            var _ray = GetLinearTransition<RTransition>();
            _ray.HeadWidth = HeadWidth;
            _ray.TailWidth = TailWidth;
            return _ray;
        }

    }
}
