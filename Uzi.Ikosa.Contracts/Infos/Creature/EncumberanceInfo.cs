using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class EncumberanceInfo
    {
        [DataMember]
        public bool Unencumbered { get; set; }
        [DataMember]
        public bool Encumbered { get; set; }
        [DataMember]
        public bool GreatlyEncumbered { get; set; }
        [DataMember]
        public bool NotWeighedDown { get; set; }
        [DataMember]
        public bool WeighedDown { get; set; }
        [DataMember]
        public bool Straining { get; set; }
        [DataMember]
        public bool OverLoaded { get; set; }
        [DataMember]
        public int Value { get; set; }
    }
}
