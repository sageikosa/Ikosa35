using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public class TargetCorner
    {
        public TargetCorner(Point3D point, params AnchorFace[] faces)
        {
            Point3D = point;
            Faces = faces;
        }

        public Point3D Point3D { get; private set; }
        public AnchorFace[] Faces { get; private set; }
    }
}
