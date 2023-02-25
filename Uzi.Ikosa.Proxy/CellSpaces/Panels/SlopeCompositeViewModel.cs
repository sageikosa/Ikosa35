using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class SlopeCompositeViewModel : BasePanelViewModel, IBaseComposite
    {
        public SlopeCompositeViewModel(SlopeCompositeInfo info, LocalMapInfo map) :
            base(info, map)
        {
        }

        public SlopeCompositeInfo SlopeCompositeInfo { get { return Info as SlopeCompositeInfo; } }

        #region IBaseComposite Members

        public void AddInnerStructures(PanelParams param, BuildableGroup group, int z, int y, int x, VisualEffect effect, Dictionary<AnchorFace, INaturalPanel> sides, List<IBasePanel> interiors)
        {
            SlopeSpaceFaces.AddInnerSlopeComposite(group, param, Thickness, SlopeCompositeInfo.SlopeThickness, effect, this);
        }

        #endregion

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterSlopeComposite(group, param, z, y, x, visibleFace, Thickness, SlopeCompositeInfo.SlopeThickness, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible && param.IsTrueSlope)
            {
                if (param.IsFaceSlopeBottom(panelFace))
                {
                    // if slope source, then all occluded
                    return true;
                }

                // otherwise only the sourceface is occluded
                return sideFace == param.SourceFace;
            }
            return false;
        }
    }
}
