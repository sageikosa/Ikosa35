using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    public interface ICellRange
    {
        IGeometricRegion GetTargetVolume(CoreActor creature, int powerLevel, IGeometricRegion source);
    }
}
