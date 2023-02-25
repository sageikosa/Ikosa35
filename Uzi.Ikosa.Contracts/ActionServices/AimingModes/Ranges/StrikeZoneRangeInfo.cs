using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class StrikeZoneRangeInfo : RangeInfo
    {
        [DataMember]
        public int MinimumReach { get; set; }
        [DataMember]
        public int MaximumReach { get; set; }
    }
}
