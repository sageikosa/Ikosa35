using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class TacticalPoint
    {
        [DataMember]
        public double Z { get; set; }
        [DataMember]
        public double Y { get; set; }
        [DataMember]
        public double X { get; set; }
    }
}
