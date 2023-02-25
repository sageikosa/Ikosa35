using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface IGetBrushByEffect
    {
        Material GetBrush(VisualEffect effect);
    }
}
