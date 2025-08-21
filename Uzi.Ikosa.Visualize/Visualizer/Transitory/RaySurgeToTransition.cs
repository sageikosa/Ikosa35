using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    public class RaySurgeToTransition : RaySurgeTransition
    {
        public RaySurgeToTransition()
            : base()
        {
        }

        protected override ScaleTransform3D InitialTransform(Point3D target)
        {
            return new ScaleTransform3D(new Vector3D(1, 1, 1), target);
        }

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            // yield
            yield return DoMakeAnimation(1, 1d, 0d);
            yield break;
        }

        public override TransientVisualizerInfo ToInfo()
        {
            return ToRayTransitionInfo<RaySurgeFromTransitionInfo>();
        }
    }
}
