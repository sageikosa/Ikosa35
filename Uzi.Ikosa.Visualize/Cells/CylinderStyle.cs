using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize
{
    [Serializable]
    public enum CylinderStyle : byte
    {
        /// <summary>Smooth curved normal surface, texture spreads over entire quarter</summary>
        Smooth = 0,
        /// <summary>Each side of a section has a corner, texture spreads over entire quarter</summary>
        Facet = 1,
        /// <summary>Each side of a section has a corner, texture repeated in each side of quarter </summary>
        Segment = 2
    }
}
