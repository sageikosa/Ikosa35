using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BackgroundCellGroupInfo : LocalCellGroupInfo
    {
        [DataMember]
        public CellSpaceInfo TemplateSpace { get; set; }
        [DataMember]
        public uint ParamData { get; set; }
    }
}
