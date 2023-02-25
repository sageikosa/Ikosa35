using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PersonalConicAimInfo : PersonalAimInfo
    {
        [DataMember]
        public double Distance { get; set; }
    }
}
