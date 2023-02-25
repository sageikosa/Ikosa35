using System;
using System.Collections.Generic;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class BaseNaturalPanel : BasePanel, INaturalPanel
    {
        protected BaseNaturalPanel(string name, SolidCellMaterial material, TileSet tiling, double thickness)
            : base(name, material, tiling, thickness)
        {
        }

        public abstract void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup,
            int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors);

        public override bool IsGas { get { return false; } }
        public override bool IsLiquid { get { return false; } }
        public override bool IsInvisible { get { return false; } }
    }
}
