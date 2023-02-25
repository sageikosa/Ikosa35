using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LocationAimInfo : RangedAimInfo
    {
        [DataMember]
        public LocationAimMode LocationAimMode { get; set; }
    }
}
