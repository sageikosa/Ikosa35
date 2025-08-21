using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public abstract class ModelAdornment
    {
        public abstract void DrawAdornment(Model3DGroup group);
    }
}
