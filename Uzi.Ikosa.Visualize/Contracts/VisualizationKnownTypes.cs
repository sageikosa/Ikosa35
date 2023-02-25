using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize.Contracts.Tactical;
using System.Reflection;

namespace Uzi.Visualize.Contracts
{
    public static class VisualizationKnownTypes
    {
        #region private static IEnumerable<Type> KnownTypes(ICustomAttributeProvider provider)
        private static IEnumerable<Type> KnownTypes(ICustomAttributeProvider provider)
        {
            // cell spaces
            yield return typeof(CylinderSpaceInfo);
            yield return typeof(SmallCylinderSpaceInfo);
            yield return typeof(SliverSpaceInfo);
            yield return typeof(SlopeSpaceInfo);
            yield return typeof(CornerSpaceInfo);
            yield return typeof(StairSpaceInfo);
            yield return typeof(CellSpaceInfo);
            yield return typeof(LFrameSpaceInfo);
            yield return typeof(PanelSpaceInfo);

            // models
            yield return typeof(MetaModel3DInfo);

            // presentations
            yield return typeof(ModelPresentationInfo);
            yield return typeof(IconPresentationInfo);
            yield return typeof(Translate3DInfo);
            yield return typeof(AxisAngleRotate3DInfo);

            // adornments
            yield return typeof(CharacterPogAdornment);

            // transient visualizations
            yield return typeof(MaterialVisualizerInfo);
            yield return typeof(TransientDelayInfo);
            yield return typeof(LinearTransitionInfo);
            yield return typeof(MarkerBallInfo);
            yield return typeof(RayTransitionInfo);
            yield return typeof(RayBoltTransitionInfo);
            yield return typeof(RaySurgeTransitionInfo);
            yield return typeof(RaySurgeToTransitionInfo);
            yield return typeof(RaySurgeFromTransitionInfo);
            yield return typeof(FlyingOrbTransitionInfo);
            yield break;
        }
        #endregion

    }
}
