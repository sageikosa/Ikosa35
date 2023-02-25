using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class DiagonalCompositeViewModel : BasePanelViewModel, IBaseComposite
    {
        public DiagonalCompositeViewModel(DiagonalCompositeInfo info, LocalMapInfo map) :
            base(info, map)
        {
        }

        public DiagonalCompositeInfo DiagonalCompositeInfo { get { return Info as DiagonalCompositeInfo; } }

        #region IBaseComposite Members

        public void AddInnerStructures(PanelParams param, BuildableGroup group, int z, int y, int x, VisualEffect effect, Dictionary<AnchorFace, INaturalPanel> sides, List<IBasePanel> interiors)
        {
            PanelSpaceFaces.AddInnerDiagonalComposite(group, param, effect, this);
        }

        #endregion

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterDiagonalComposite(group, param, z, y, x, panelFace, effect, this, bump);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                if (param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace))
                {
                    // full side acts like a normal panel
                    return true;
                }
                else if (param.IsFaceDiagonalSide(panelFace))
                {
                    return param.DiagonalFaceControlFaces(panelFace).Contains(sideFace);
                }
                else if (param.IsFaceTriangularSink(panelFace))
                {
                    return param.TriangularSinkEdges(panelFace).Contains(sideFace);
                }
            }
            return false;
        }
    }
}
