using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class SphereBuilderInfo : BuilderInfo
    {
        public SphereBuilderInfo()
        {
        }

        public SphereBuilderInfo(SphereBuilder builder)
        {
            Radius = builder.Radius;
        }

        [DataMember]
        public int Radius { get; set; }

        public override Visualize.IGeometryBuilder GetBuilder()
        {
            return new SphereBuilder(Radius);
        }
    }
}
