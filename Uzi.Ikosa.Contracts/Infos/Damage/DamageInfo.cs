using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DamageInfo
    {
        public DamageInfo()
        {
        }

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public string Extra { get; set; }
    }
}
