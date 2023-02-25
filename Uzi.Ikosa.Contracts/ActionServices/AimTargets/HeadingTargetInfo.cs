using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used primarily for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class HeadingTargetInfo : AimTargetInfo
    {
        public HeadingTargetInfo()
            : base()
        {
        }

        [DataMember]
        public int Heading { get; set; }

        [DataMember]
        public int UpDownAdjust { get; set; }
    }
}
