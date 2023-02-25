using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class WallSurfaceAimInfo : RangedAimInfo
    {
        [DataMember]
        public RangeInfo MaxLength { get; set; }
        [DataMember]
        public RangeInfo MaxHeight { get; set; }
        [DataMember]
        public RangeInfo MinFaceSquareSide { get; set; }
    }
}
