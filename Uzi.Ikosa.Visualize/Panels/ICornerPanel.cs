using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface ICornerPanel : IBasePanel
    {
        double Offset { get; }
    }
}
