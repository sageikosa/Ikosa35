using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LFrameSpaceInfo : CellSpaceInfo
    {
        #region construction
        public LFrameSpaceInfo()
        {
        }

        public LFrameSpaceInfo(ILFrameSpace lFrame)
            : base(lFrame)
        {
            PlusMaterial = lFrame.PlusMaterialName;
                PlusTiling = lFrame.PlusTilingName;
            Thickness = lFrame.Thickness;
            Width1 = lFrame.Width1;
            Width2 = lFrame.Width2;
            IsPlusGas = lFrame.IsPlusGas;
            IsPlusInvisible = lFrame.IsPlusInvisible;
            IsPlusLiquid = lFrame.IsPlusLiquid;
            IsPlusSolid = lFrame.IsPlusSolid;
        }
        #endregion

        [DataMember]
        public string PlusMaterial { get; set; }
        [DataMember]
        public string PlusTiling { get; set; }
        [DataMember]
        public double Thickness { get; set; }
        [DataMember]
        public double Width1 { get; set; }
        [DataMember]
        public double Width2 { get; set; }
        [DataMember]
        public bool IsPlusGas { get; set; }
        [DataMember]
        public bool IsPlusLiquid { get; set; }
        [DataMember]
        public bool IsPlusInvisible { get; set; }
        [DataMember]
        public bool IsPlusSolid { get; set; }
    }
}
