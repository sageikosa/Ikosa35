using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class CornerPanelViewModel : BasePanelViewModel, INaturalPanel
    {
        public CornerPanelViewModel(CornerPanelInfo info, LocalMapInfo map)
            :base(info, map)
        {
        }

        public CornerPanelInfo CornerPanelInfo { get { return Info as CornerPanelInfo; } }

        #region INaturalPanel Members

        public void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup group, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            PanelSpaceFaces.AddInnerCornerPanel(group, panelFace, Thickness, param.GetPanelEdge(panelFace), CornerPanelInfo.Offset, effect, this);
        }

        #endregion

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterCornerPanel(group,z,y,x, panelFace, visibleFace, Thickness, param.GetPanelEdge(panelFace), CornerPanelInfo.Offset, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                var _snapped = panelFace.GetSnappedFace(param.GetPanelEdge(panelFace));
                return (_snapped == sideFace);
            }
            return false;
        }
    }
}
