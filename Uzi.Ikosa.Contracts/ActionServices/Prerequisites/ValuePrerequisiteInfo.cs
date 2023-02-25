using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ValuePrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public int? Value { get; set; }
    }
}
