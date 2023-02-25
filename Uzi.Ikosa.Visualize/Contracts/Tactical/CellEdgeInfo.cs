using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class CellEdgeInfo
    {
        public CellEdgeInfo()
        {
        }

        public CellEdgeInfo(ICellEdge edge)
        {
            EdgeMaterial = edge.EdgeMaterial;
            EdgeTiling = edge.EdgeTiling;
            Width = edge.Width;
        }

        [DataMember]
        public string EdgeMaterial { get; set; }

        [DataMember]
        public string EdgeTiling { get; set; }

        [DataMember]
        public double Width { get; set; }
    }
}
