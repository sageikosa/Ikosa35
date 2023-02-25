using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    interface IGetCellStructure
    {
        LocalCellGroupInfo Info { get; }
        CellStructureInfo? GetContainedCellSpace(int z, int y, int x);
    }
}
