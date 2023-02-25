using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CheckPrerequisiteInfo : ValuePrerequisiteInfo
    {
        [DataMember]
        public int VoluntaryPenalty { get; set; }

        [DataMember]
        public bool IsUsingPenalty { get; set; }
    }
}
