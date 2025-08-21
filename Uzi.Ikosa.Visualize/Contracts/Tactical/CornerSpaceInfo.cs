using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CornerSpaceInfo : CellSpaceInfo
    {
        public CornerSpaceInfo()
        {
        }

        public CornerSpaceInfo(IWedgeSpace corner)
            : base(corner)
        {
            PlusMaterial = corner.PlusMaterialName;
            PlusTiling = corner.PlusTilingName;
            Offset1 = corner.Offset1;
            Offset2 = corner.Offset2;
            IsWedge = !(corner.CornerStyle);
            IsPlusGas = corner.IsPlusGas;
            IsPlusSolid = corner.IsPlusSolid;
            IsPlusLiquid = corner.IsPlusLiquid;
            IsPlusInvisible = corner.IsPlusInvisible;
        }

        [DataMember]
        public string PlusMaterial { get; set; }
        [DataMember]
        public string PlusTiling { get; set; }
        [DataMember]
        public bool IsPlusGas { get; set; }
        [DataMember]
        public bool IsPlusSolid { get; set; }
        [DataMember]
        public bool IsPlusLiquid { get; set; }
        [DataMember]
        public bool IsPlusInvisible { get; set; }
        [DataMember]
        public double Offset1 { get; set; }
        [DataMember]
        public double Offset2 { get; set; }
        [DataMember]
        public bool IsWedge { get; set; }
    }
}