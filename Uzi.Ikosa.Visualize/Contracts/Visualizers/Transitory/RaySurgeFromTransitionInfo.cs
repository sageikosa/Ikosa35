using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RaySurgeFromTransitionInfo : RaySurgeTransitionInfo
    {
        public override TransientVisualizer GetTransientVisualizer()
        {
            return GetRayTransition<RaySurgeFromTransition>();
        }
    }
}
