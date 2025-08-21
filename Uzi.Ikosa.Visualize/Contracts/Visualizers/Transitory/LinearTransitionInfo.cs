using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LinearTransitionInfo : MaterialVisualizerInfo
    {
        [DataMember]
        public Point3D Target { get; set; }

        protected LTransition GetLinearTransition<LTransition>()
            where LTransition : LinearTransition, new()
        {
            var _lTransition = GetMaterialTransientVisualizer<LTransition>();
            _lTransition.Target = Target;
            return _lTransition;
        }
    }
}
