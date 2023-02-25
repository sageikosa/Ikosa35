using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AwarenessAimInfo : RangedAimInfo
    {
        [DataMember]
        public TargetTypeInfo[] ValidTargetTypes { get; set; }
        [DataMember]
        public bool AllowDuplicates { get; set; }
    }
}
