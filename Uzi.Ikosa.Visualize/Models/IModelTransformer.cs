using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>
    /// 1: size; 2: twist; 3: tilt; 4: heading; 5: custom; 6: global-position; 7: base-face bind; 8: model-offset
    /// </summary>
    public interface IModelTransformer : IPresentation
    {
        Vector3D CubeFitScale { get; }
        double ApparentScale { get; }
        double Twist { get; }
        double Tilt { get; }
        Vector3D TiltAxis { get; }
        double Pivot { get; }
        Vector3D IntraModelOffset { get; }
        Transform3DGroup CustomTransforms { get; }
        bool IsAdjustingPosition { get; }
        bool IsFullOrigin { get; }
        double TiltElevation { get; }
    }
}
