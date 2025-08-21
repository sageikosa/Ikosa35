using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public abstract class Transform3DInfo
    {
        public abstract Transform3D ToTransform3D();
    }
}
