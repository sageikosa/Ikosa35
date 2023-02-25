using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class Take10Info
    {
        [DataMember]
        public double RemainingRounds { get; set; }
    }
}
