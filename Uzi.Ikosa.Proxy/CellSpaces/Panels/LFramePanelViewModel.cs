using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class LFramePanelViewModel : BasePanelViewModel, INaturalPanel
    {
        public LFramePanelViewModel(LFramePanelInfo info, LocalMapInfo map)
            : base(info, map)
        {
        }

        public LFramePanelInfo LFramePanelInfo { get { return Info as LFramePanelInfo; } }

        #region INaturalPanel Members

        public void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup group, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            PanelSpaceFaces.AddInnerLFramePanel(group, panelFace, Thickness, param.GetPanelCorner(panelFace),
                LFramePanelInfo.HorizontalWidth, LFramePanelInfo.VerticalWidth, effect, this);
        }

        #endregion

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterLFramePanel(group, z, y, x, panelFace, visibleFace, Thickness, param.GetPanelCorner(panelFace),
                LFramePanelInfo.HorizontalWidth, LFramePanelInfo.VerticalWidth, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                if (param.GetPanelType(panelFace) == PanelType.MaskedLFrame)
                    return true;
                var _edges = param.GetPanelCorner(panelFace).GetEdgeFaces(panelFace);
                return _edges.Contains(sideFace);
            }
            return false;
        }
    }
}
