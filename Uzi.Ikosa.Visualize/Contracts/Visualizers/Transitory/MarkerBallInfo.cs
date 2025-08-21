using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MarkerBallInfo : MaterialVisualizerInfo
    {
        [DataMember]
        public int ThetaDiv { get; set; }

        [DataMember]
        public int PhiDiv { get; set; }

        /// <summary>Initial Radius for sphere</summary>
        [DataMember]
        public double StartRadius { get; set; }

        /// <summary>Final Radius for sphere</summary>
        [DataMember]
        public double EndRadius { get; set; }

        public override TransientVisualizer GetTransientVisualizer()
        {
            var _mBall = GetMaterialTransientVisualizer<MarkerBall>();
            _mBall.ThetaDiv = ThetaDiv;
            _mBall.PhiDiv = PhiDiv;
            _mBall.StartRadius = StartRadius;
            _mBall.EndRadius = EndRadius;
            return _mBall;
        }
    }
}
