using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    public interface ICellStructureProvider
    {
        bool CanCaptureCellStructure();
        ref readonly CellStructure GetCellStructure();
    }
}
