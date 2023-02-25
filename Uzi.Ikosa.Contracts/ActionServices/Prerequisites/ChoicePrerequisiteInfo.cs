using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ChoicePrerequisiteInfo : PrerequisiteInfo
    {
        [DataMember]
        public OptionAimOption[] Choices { get; set; }
        [DataMember]
        public OptionAimOption Selected { get; set; }
    }
}
