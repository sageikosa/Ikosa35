using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CornerPanelInfo : BasePanelInfo
    {
        public CornerPanelInfo()
            : base()
        {
        }

        public CornerPanelInfo(ICornerPanel corner)
            : base(corner)
        {
            Offset = corner.Offset;
        }

        [DataMember]
        public double Offset { get; set; }
    }
}
