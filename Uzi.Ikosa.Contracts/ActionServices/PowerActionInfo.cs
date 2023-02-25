using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PowerActionInfo : ActionInfo
    {
        [DataMember]
        public PowerDefInfo PowerDef { get; set; }

        [DataMember]
        public int PowerLevel { get; set; }
    }
}
