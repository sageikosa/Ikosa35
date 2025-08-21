using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class TileSetInfo
    {
        [DataMember]
        public string CellMaterial { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string BrushCollectionKey { get; set; }

        [DataMember]
        public int XMinusIndex { get; set; }

        [DataMember]
        public int XPlusIndex { get; set; }

        [DataMember]
        public int YMinusIndex { get; set; }

        [DataMember]
        public int YPlusIndex { get; set; }

        [DataMember]
        public int ZMinusIndex { get; set; }

        [DataMember]
        public int ZPlusIndex { get; set; }

        [DataMember]
        public int WedgeIndex { get; set; }

        [DataMember]
        public int InnerIndex { get; set; }
    }
}
