using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    /// <summary>NoPanel | Panel1 | Panel2 | Panel3 | Corner | LFrame | MaskedCorner | MaskedLFrame</summary>
    public enum PanelType : byte
    {
        NoPanel = 0,
        Panel1 = 1,
        Panel2 = 2,
        Panel3 = 3,
        Corner = 4,
        LFrame = 5,
        MaskedCorner = 6,
        MaskedLFrame = 7
    }

    public static class PanelTypeHelper
    {
        /// <summary>PanelType is one of NoPanel | Panel1 | Panel2 | Panel3</summary>
        public static bool IsInteriorBindable(this PanelType self)
        {
            return (self == PanelType.NoPanel)
                || (self == PanelType.Panel1)
                || (self == PanelType.Panel2)
                || (self == PanelType.Panel3);
        }
    }
}