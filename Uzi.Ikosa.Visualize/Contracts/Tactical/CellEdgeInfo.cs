using System.Runtime.Serialization;

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
