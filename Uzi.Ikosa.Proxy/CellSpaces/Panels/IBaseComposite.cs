using System.Collections.Generic;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    /// <summary>Marker interface</summary>
    public interface IBaseComposite
    {
        void AddInnerStructures(PanelParams param, BuildableGroup group, int z, int y, int x, VisualEffect effect,
            Dictionary<AnchorFace, INaturalPanel> sides, List<IBasePanel> interiors);
    }
}
