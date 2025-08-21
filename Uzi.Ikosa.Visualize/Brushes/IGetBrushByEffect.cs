using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface IGetBrushByEffect
    {
        Material GetBrush(VisualEffect effect);
    }
}
