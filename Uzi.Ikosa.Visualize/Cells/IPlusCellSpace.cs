using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface IPlusCellSpace : ICellSpace
    {
        string PlusMaterialName { get; }
        string PlusTilingName { get; }
        bool IsPlusGas { get; }
        bool IsPlusSolid { get; }
        bool IsPlusLiquid { get; }
        bool IsPlusInvisible { get; }
        BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect);
        BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect);
        BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect);
    }
}
