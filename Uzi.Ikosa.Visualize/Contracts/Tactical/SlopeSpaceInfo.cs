using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class SlopeSpaceInfo : SliverSpaceInfo
    {
        public SlopeSpaceInfo()
        {
        }

        public SlopeSpaceInfo(IPlusCellSpace sliver, ICellEdge edge)
            : base(sliver, edge)
        {
        }
    }
}
