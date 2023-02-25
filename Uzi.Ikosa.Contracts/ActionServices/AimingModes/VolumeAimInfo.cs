using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class VolumeAimInfo : RangedAimInfo
    {
        [DataMember]
        public RangeInfo CubeSize { get; set; }
    }
}
