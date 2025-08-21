using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MaterialFillInfo : BasePanelInfo
    {
        public MaterialFillInfo()
            : base()
        {
        }

        public MaterialFillInfo(IBasePanel fill)
            : base(fill)
        {
        }
    }
}
