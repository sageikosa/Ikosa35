using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    public class RaySurgeFromTransition : RaySurgeTransition
    {
        public RaySurgeFromTransition()
            : base()
        {
        }

        protected override ScaleTransform3D InitialTransform(Point3D target)
        {
            return new ScaleTransform3D(new Vector3D(1, 1, 0), Source);
        }

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            // yield
            yield return DoMakeAnimation(0, 0d, 1d);
            yield break;
        }

        public override TransientVisualizerInfo ToInfo()
        {
            return ToRayTransitionInfo<RaySurgeFromTransitionInfo>();
        }
    }
}
