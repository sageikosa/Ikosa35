using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace =Statics.Namespace)]
    public class CubicTargetInfo : AimTargetInfo
    {
        public CubicTargetInfo()
            : base()
        {
        }

        [DataMember]
        public CubicInfo Cubic { get; set; }
    }
}
