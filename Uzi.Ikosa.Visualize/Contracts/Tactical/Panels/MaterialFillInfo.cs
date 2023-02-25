using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
