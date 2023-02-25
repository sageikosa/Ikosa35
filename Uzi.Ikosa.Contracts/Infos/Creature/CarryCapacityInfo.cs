using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CarryCapacityInfo
    {
        [DataMember]
        public double LightLoadLimit { get; set; }
        [DataMember]
        public double MediumLoadLimit { get; set; }
        [DataMember]
        public double HeavyLoadLimit { get; set; }
        [DataMember]
        public double LiftOverHeadLimit { get; set; }
        [DataMember]
        public double LiftOffGroundLimit { get; set; }
        [DataMember]
        public double PushOrDragLimit { get; set; }
    }
}
