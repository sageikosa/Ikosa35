using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class LocationTargetInfo : AimTargetInfo
    {
        public LocationTargetInfo()
            : base()
        {
        }

        [DataMember]
        public CellInfo CellInfo { get; set; }

        [DataMember]
        public LocationAimMode LocationAimMode { get; set; }

        public Point3D GetPoint3D()
            => LocationAimMode.GetPoint3D(CellInfo);
    }
}
