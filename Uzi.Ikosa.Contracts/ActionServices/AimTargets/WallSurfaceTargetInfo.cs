using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class WallSurfaceTargetInfo : LocationTargetInfo
    {
        public WallSurfaceTargetInfo()
            : base()
        {
        }

        [DataMember]
        public int AnchorFace { get; set; }
    }
}
