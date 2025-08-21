using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class RaySurgeToTransitionInfo : RaySurgeTransitionInfo
    {
        public override TransientVisualizer GetTransientVisualizer()
        {
            return GetRayTransition<RaySurgeToTransition>();
        }
    }
}
