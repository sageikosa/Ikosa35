using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class MaterialFillViewModel : BasePanelViewModel
    {
        public MaterialFillViewModel(MaterialFillInfo info, LocalMapInfo map) :
            base(info, map)
        {
        }

        public MaterialFillInfo SlopeCompositeInfo { get { return Info as MaterialFillInfo; } }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            if (IsInvisible)
                return;

            PanelSpaceFaces.AddOuterMaterialFill(group, z, y, x, panelFace, this, effect, bump);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            // fill never blocks real sides
            return false;
        }
    }
}
