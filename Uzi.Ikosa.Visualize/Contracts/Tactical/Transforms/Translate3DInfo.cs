using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class Translate3DInfo : Transform3DInfo
    {
        [DataMember]
        public double OffsetX { get; set; }
        [DataMember]
        public double OffsetY { get; set; }
        [DataMember]
        public double OffsetZ { get; set; }

        public override Transform3D ToTransform3D()
            => new TranslateTransform3D(OffsetX, OffsetY, OffsetZ);
    }
}
