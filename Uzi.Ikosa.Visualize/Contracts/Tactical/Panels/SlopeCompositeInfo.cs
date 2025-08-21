using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SlopeCompositeInfo : BasePanelInfo
    {
        public SlopeCompositeInfo()
            : base()
        {
        }

        public SlopeCompositeInfo(ISlopeComposite slopeComposite)
            : base(slopeComposite)
        {
            SlopeThickness = slopeComposite.SlopeThickness;
        }

        [DataMember]
        public double SlopeThickness { get; set; }
    }
}
