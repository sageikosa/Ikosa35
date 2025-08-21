using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CellSpaceInfo
    {
        #region construction
        public CellSpaceInfo()
        {
        }

        public CellSpaceInfo(ICellSpace cellSpace)
        {
            Index = cellSpace.Index;
            Name = cellSpace.Name;
            CellMaterial = cellSpace.CellMaterialName;
            Tiling = cellSpace.TilingName;
            IsGas = cellSpace.IsGas;
            IsLiquid = cellSpace.IsLiquid;
            IsSolid = cellSpace.IsSolid;
            IsInvisible = cellSpace.IsInvisible;
        }
        #endregion

        [DataMember]
        public uint Index { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string CellMaterial { get; set; }
        [DataMember]
        public string Tiling { get; set; }
        [DataMember]
        public bool IsGas { get; set; }
        [DataMember]
        public bool IsLiquid { get; set; }
        [DataMember]
        public bool IsInvisible { get; set; }
        [DataMember]
        public bool IsSolid { get; set; }
    }
}
