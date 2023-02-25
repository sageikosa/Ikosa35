using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RangedAimInfo : AimingModeInfo
    {
        [DataMember]
        public double Range { get; set; }
        [DataMember]
        public RangeInfo RangeInfo { get; set; }
    }
}
