using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface INaturalPanel : IBasePanel
    {
        void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors);
    }
}
