using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface ILFramePanel : IBasePanel
    {
        double HorizontalWidth { get; }
        double VerticalWidth { get; }
    }
}
