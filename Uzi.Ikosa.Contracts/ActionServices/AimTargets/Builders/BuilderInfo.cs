using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public abstract class BuilderInfo
    {
        public abstract IGeometryBuilder GetBuilder();
    }
}
