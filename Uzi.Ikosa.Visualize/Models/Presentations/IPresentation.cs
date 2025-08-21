using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface IPresentation : ICellLocation, IGeometricSize
    {
        List<VisualEffectValue> VisualEffects { get; }
        AnchorFace BaseFace { get; }
        Vector3D MoveFrom { get; }
    }
}
