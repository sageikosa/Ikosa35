using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SavePrerequisiteInfo : ValuePrerequisiteInfo
    {
        [DataMember]
        public string SaveType { get; set; }
    }
}
