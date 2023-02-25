using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class CylinderBuilderInfo : BuilderInfo
    {
        public CylinderBuilderInfo()
        {
        }

        public CylinderBuilderInfo(CylinderBuilder builder)
        {
            Radius = builder.Radius;
            Depth = builder.Depth;
        }

        [DataMember]
        public int Radius { get; set; }
        [DataMember]
        public int Depth { get; set; }

        public override IGeometryBuilder GetBuilder()
        {
            return new CylinderBuilder(this.Radius, this.Depth);
        }
    }
}
