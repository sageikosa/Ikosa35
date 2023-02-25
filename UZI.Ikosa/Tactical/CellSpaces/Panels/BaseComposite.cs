using System;
using System.Collections.Generic;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class BaseComposite : BasePanel, IBasePanel
    {
        protected BaseComposite(string name, SolidCellMaterial material, TileSet tiling, double thickness)
            : base(name, material, tiling, thickness)
        {
        }

        public abstract void AddInnerStructures(PanelParams param, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, Dictionary<AnchorFace, INaturalPanel> naturalSides, IEnumerable<IBasePanel> interiors);

    }
}
