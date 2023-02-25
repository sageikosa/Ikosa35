using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PanelShadingInfo : CellInfo
    {
        [DataMember]
        public int AnchorFaceValue { get; set; }

        [DataMember]
        public ulong CellSpace { get; set; }

        // TODO: put [EnumMember] attributes on AnchorFace
        public AnchorFace AnchorFace
        {
            get => (AnchorFace)AnchorFaceValue;
            set => AnchorFaceValue = (int)value;
        }
    }
}
