using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public interface ISlopeComposite : IBasePanel
    {
        double SlopeThickness { get; }
    }
}
