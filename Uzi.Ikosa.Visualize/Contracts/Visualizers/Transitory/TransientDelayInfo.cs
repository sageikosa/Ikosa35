using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class TransientDelayInfo : TransientVisualizerInfo
    {
        public override TransientVisualizer GetTransientVisualizer()
        {
            return GetRootTransientVisualizer<TransientDelay>();
        }
    }
}
