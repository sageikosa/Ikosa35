using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class FlyingOrbTransitionInfo : LinearTransitionInfo
    {
        [DataMember]
        public double Radius { get; set; }
        [DataMember]
        public int ThetaDiv { get; set; }
        [DataMember]
        public int PhiDiv { get; set; }

        public override TransientVisualizer GetTransientVisualizer()
        {
            var _orb = GetLinearTransition<FlyingOrbTransition>();
            _orb.ThetaDiv = ThetaDiv;
            _orb.PhiDiv = PhiDiv;
            _orb.Radius = Radius;
            return _orb;
        }
    }
}
