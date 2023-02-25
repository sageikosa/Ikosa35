using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class GeometricTargetInfo : AimTargetInfo
    {
        public GeometricTargetInfo()
            : base()
        {
        }

        [DataMember]
        public CellInfo Origin { get; set; }

        [DataMember]
        public BuilderInfo Builder { get; set; }

        [DataMember]
        public CellInfo Location { get; set; }
    }
}
