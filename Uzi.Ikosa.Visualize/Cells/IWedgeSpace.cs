using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public interface IWedgeSpace : IPlusCellSpace
    {
        double Offset1 { get; }
        double Offset2 { get; }
        bool CornerStyle { get; }
        bool HasTiling { get; }
        bool HasPlusTiling { get; }
    }
}
