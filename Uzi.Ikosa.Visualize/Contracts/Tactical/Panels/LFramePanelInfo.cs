using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LFramePanelInfo : BasePanelInfo
    {
        public LFramePanelInfo()
            : base()
        {
        }

        public LFramePanelInfo(ILFramePanel lFrame)
            : base(lFrame)
        {
            HorizontalWidth = lFrame.HorizontalWidth;
            VerticalWidth = lFrame.VerticalWidth;
        }

        [DataMember]
        public double HorizontalWidth { get; set; }
        [DataMember]
        public double VerticalWidth { get; set; }
    }
}
