using Uzi.Visualize;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public struct LocatedEffect : ICellLocation
    {
        [DataMember]
        public int Z { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public VisualEffect Effect { get; set; }
        public CellPosition ToCellPosition() => new CellPosition(this);
    }
}
