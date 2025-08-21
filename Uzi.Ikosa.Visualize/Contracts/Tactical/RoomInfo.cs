using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RoomInfo : LocalCellGroupInfo
    {
        [DataMember]
        public ulong[] CellIDs { get; set; }
    }
}
