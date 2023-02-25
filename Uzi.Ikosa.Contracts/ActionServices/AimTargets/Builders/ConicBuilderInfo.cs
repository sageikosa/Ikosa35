using System.Linq;
using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class ConicBuilderInfo : BuilderInfo
    {
        public ConicBuilderInfo()
        {
        }

        public ConicBuilderInfo(ConeBuilder builder)
        {
            Radius = builder.Radius;
            CrossingFaces = builder.CrossingFaces.Select(_f => (int)_f).ToArray();
        }

        [DataMember]
        public int[] CrossingFaces { get; set; }

        [DataMember]
        public int Radius { get; set; }

        public override IGeometryBuilder GetBuilder()
        {
            return new ConeBuilder(this.Radius, this.CrossingFaces.Select(_f => (AnchorFace)_f).ToArray());
        }
    }
}
