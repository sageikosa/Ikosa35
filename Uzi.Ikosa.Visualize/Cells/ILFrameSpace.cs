using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface ILFrameSpace : ICellSpace
    {
        string PlusMaterialName { get; }
        string PlusTilingName { get; }
        double Thickness { get; }
        double Width1 { get; }
        double Width2 { get; }
        bool IsPlusGas { get; }
        bool IsPlusInvisible { get; }
        bool IsPlusLiquid { get; }
        bool IsPlusSolid { get; }
    }
}
