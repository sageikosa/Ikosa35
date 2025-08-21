using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class StairSpaceInfo : CellSpaceInfo
    {
        #region construction
        public StairSpaceInfo()
        {
        }

        public StairSpaceInfo(IStairs stair)
            : base(stair)
        {
            PlusMaterial = stair.PlusMaterialName;
                PlusTiling = stair.PlusTilingName;
            Steps = stair.Steps;
            IsPlusGas = stair.IsPlusGas;
            IsPlusInvisible = stair.IsPlusInvisible;
            IsPlusLiquid = stair.IsPlusLiquid;
            IsPlusSolid = stair.IsPlusSolid;
        }
        #endregion

        [DataMember]
        public string PlusMaterial { get; set; }
        [DataMember]
        public string PlusTiling { get; set; }
        [DataMember]
        public int Steps { get; set; }
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
