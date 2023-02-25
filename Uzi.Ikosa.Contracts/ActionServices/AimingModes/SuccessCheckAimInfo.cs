using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SuccessCheckAimInfo : AimingModeInfo
    {
        [DataMember]
        public int VoluntaryPenalty { get; set; }
    }
}
