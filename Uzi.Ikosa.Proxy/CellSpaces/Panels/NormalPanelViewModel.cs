using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class NormalPanelViewModel : BasePanelViewModel, INaturalPanel
    {
        public NormalPanelViewModel(NormalPanelInfo info, LocalMapInfo map)
            : base(info, map)
        {
        }

        public NormalPanelInfo NormalPanelInfo { get { return Info as NormalPanelInfo; } }

        #region INaturalPanel Members

        public void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup group, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            PanelSpaceFaces.AddInnerNormalPanel(group, panelFace, Thickness, effect, this);
        }

        #endregion

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            if (panelFace == visibleFace)
                PanelSpaceFaces.AddOuterNormalPanel(group, new CellPosition(z, y, x), panelFace, effect, this);
            else
                PanelSpaceFaces.AddOuterNormalPanel(group, z, y, x, panelFace, visibleFace, Thickness, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            // blocks exposure as long as the panel is not invisible
            return !IsInvisible;
        }
    }
}
