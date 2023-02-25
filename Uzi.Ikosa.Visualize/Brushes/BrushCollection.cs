using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Uzi.Visualize
{
    public class BrushCollection : Collection<BrushDefinition>
    {
        public void RefreshAll()
        {
            foreach (var _brush in this)
                _brush.ClearCache();
        }

        public BrushCollection AddRange(IEnumerable<BrushDefinition> range)
        {
            foreach (var _brushDef in range)
                this.Add(_brushDef);
            return this;
        }
    }
}
