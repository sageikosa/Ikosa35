using System.Collections.Generic;

namespace Uzi.Visualize
{
    public interface INaturalPanel : IBasePanel
    {
        void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors);
    }
}
