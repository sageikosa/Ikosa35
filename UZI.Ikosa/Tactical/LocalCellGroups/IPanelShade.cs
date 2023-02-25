using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Tactical
{
    public interface IPanelShade : ICore
    {
        IEnumerable<PanelShadingInfo> GetPanelShadings(ISensorHost sensorHost, bool insideGroup, ICellLocation location);
    }
}
