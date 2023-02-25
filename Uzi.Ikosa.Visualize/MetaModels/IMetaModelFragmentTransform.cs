using System;
using System.Windows.Media.Media3D;
namespace Uzi.Visualize
{
    public interface IMetaModelFragmentTransform
    {
        bool NoseUp { get; set; }
        double? Roll { get; set; }
        double? Pitch { get; set; }
        double? Yaw { get; set; }
        Vector3D? OriginOffset { get; set; }
        Vector3D? Scale { get; set; }
        Vector3D? Offset { get; set; }
    }
}
