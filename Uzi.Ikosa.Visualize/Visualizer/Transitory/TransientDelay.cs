using System;
using System.Windows.Media.Animation;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    public class TransientDelay : TransientVisualizer
    {
        public TransientDelay()
            : base()
        {
        }

        protected override Timeline GetDirectAnimations(Visualization visualization)
        {
            return new ParallelTimeline(TimeSpan.FromTicks(0), Duration);
        }

        public override TransientVisualizerInfo ToInfo()
        {
            return ToTransientVisualizerInfo<TransientDelayInfo>();
        }
    }
}
