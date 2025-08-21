using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class SliverSpaceInfo : CellSpaceInfo
    {
        public SliverSpaceInfo()
        {
        }

        public SliverSpaceInfo(IPlusCellSpace sliver, ICellEdge cellEdge)
            : base(sliver)
        {
            PlusMaterial = sliver.PlusMaterialName;
            PlusTiling = sliver.PlusTilingName;
            IsPlusGas = sliver.IsPlusGas;
            IsPlusSolid = sliver.IsPlusSolid;
            IsPlusLiquid = sliver.IsPlusLiquid;
            IsPlusInvisible = sliver.IsPlusInvisible;
            CellEdge = new CellEdgeInfo(cellEdge);
        }

        [DataMember]
        public bool IsPlusGas { get; set; }
        [DataMember]
        public bool IsPlusSolid { get; set; }
        [DataMember]
        public bool IsPlusLiquid { get; set; }
        [DataMember]
        public bool IsPlusInvisible { get; set; }
        [DataMember]
        public string PlusMaterial { get; set; }
        [DataMember]
        public string PlusTiling { get; set; }
        [DataMember]
        public CellEdgeInfo CellEdge { get; set; }
    }
}
