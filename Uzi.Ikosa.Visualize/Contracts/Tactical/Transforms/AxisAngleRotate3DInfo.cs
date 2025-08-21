using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class AxisAngleRotate3DInfo : Transform3DInfo
    {
        [DataMember]
        public double CenterX { get; set; }
        [DataMember]
        public double CenterY { get; set; }
        [DataMember]
        public double CenterZ { get; set; }
        [DataMember]
        public double AxisX { get; set; }
        [DataMember]
        public double AxisY { get; set; }
        [DataMember]
        public double AxisZ { get; set; }
        [DataMember]
        public double Angle { get; set; }

        public override Transform3D ToTransform3D()
            => new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(AxisX, AxisY, AxisZ), Angle), 
                CenterX, CenterY, CenterZ);
    }
}
