using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class NormalPanelInfo : BasePanelInfo
    {
        public NormalPanelInfo()
            : base()
        {
        }

        public NormalPanelInfo(IBasePanel panel)
            : base(panel)
        {
        }
    }
}
