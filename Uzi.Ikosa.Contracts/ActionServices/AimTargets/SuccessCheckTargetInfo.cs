using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class SuccessCheckTargetInfo : AimTargetInfo
    {
        public SuccessCheckTargetInfo()
            : base()
        {
        }

        [DataMember]
        public int? CheckRoll { get; set; }

        [DataMember]
        public bool IsUsingPenalty { get; set; }
    }
}
