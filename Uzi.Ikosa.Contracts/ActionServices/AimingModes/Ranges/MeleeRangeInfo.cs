using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: range value listed as grid cells, not map units
    [DataContract(Namespace = Statics.Namespace)]
    public class MeleeRangeInfo : RangeInfo
    {
        [DataMember]
        public int ReachSquares { get; set; }
    }
}
