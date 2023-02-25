using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface ICellSpace
    {
        uint Index { get; }
        string Name { get; }
        string CellMaterialName { get; }
        string TilingName { get; }
        bool IsGas { get; }
        bool IsSolid { get; }
        bool IsLiquid { get; }
        bool IsInvisible { get; }
        bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentRegion);
        bool? ShowCubicFace(uint param, AnchorFace outwardFace);
        bool ShowDirectionalFace(uint param, AnchorFace outwardFace);
        bool? OccludesFace(uint param, AnchorFace outwardFace);
        BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect);
        BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect);
        BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect);
    }
}
