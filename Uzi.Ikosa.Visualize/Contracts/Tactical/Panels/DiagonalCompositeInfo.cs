using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DiagonalCompositeInfo : BasePanelInfo
    {
        public DiagonalCompositeInfo()
            : base()
        {
        }

        public DiagonalCompositeInfo(IBasePanel diagonalComposite)
            : base(diagonalComposite)
        {
        }
    }
}
