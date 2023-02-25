using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface IStairs : ICellSpace
    {
        string PlusMaterialName { get; }
        string PlusTilingName { get; }
        int Steps { get; }
        bool IsPlusGas { get; }
        bool IsPlusLiquid { get; }
        bool IsPlusInvisible { get; }
        bool IsPlusSolid { get; }
    }
}
