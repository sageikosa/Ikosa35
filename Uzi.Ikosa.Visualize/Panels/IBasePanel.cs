using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Packaging;

namespace Uzi.Visualize
{
    public interface IBasePanel : ICorePart
    {
        void AddOuterSurface(PanelParams param, BuildableGroup pair, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels);
        bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace);
        bool IsGas { get; }
        bool IsSolid { get; }
        bool IsLiquid { get; }
        bool IsInvisible { get; }
        bool OutwardVisible(int towardsZ, int towardsY, int towardsX);
        bool InwardVisible(int towardsZ, int towardsY, int towardsX);
        double Thickness { get; }
        string MaterialName { get; }
        string TilingName { get; }
        BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect);
        BuildableMeshKey GetBuildableMeshKey(SideIndex side, VisualEffect effect);
        BuildableMaterial GetSideFaceMaterial(SideIndex side, VisualEffect effect);
        BuildableMaterial GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect);
        BuildableMaterial GetWedgeMaterial(VisualEffect effect);
    }
}
