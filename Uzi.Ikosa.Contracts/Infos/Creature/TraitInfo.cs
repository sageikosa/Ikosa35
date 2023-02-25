using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class TraitInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Benefit { get; set; }
        [DataMember]
        public string TraitNature { get; set; }
        [DataMember]
        public string TraitCategory { get; set; }
    }
}
