using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BasePanelInfo
    {
        #region construction
        public BasePanelInfo()
        {
        }

        public BasePanelInfo(IBasePanel panel)
        {
            Name = panel.Name;
            Tiling = panel.TilingName;
            Material = panel.MaterialName;
            Thickness = panel.Thickness;
            IsGas = panel.IsGas;
            IsLiquid = panel.IsLiquid;
            IsSolid = panel.IsSolid;
            IsInvisible = panel.IsInvisible;
        }
        #endregion

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Material { get; set; }
        [DataMember]
        public string Tiling { get; set; }
        [DataMember]
        public double Thickness { get; set; }
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
