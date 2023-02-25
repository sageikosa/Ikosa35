using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class SlopeSpaceViewModel : SliverSpaceViewModel
    {
        public SlopeSpaceViewModel(SlopeSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
        }

        public SlopeSpaceInfo SlopeInfo
            => SliverInfo as SlopeSpaceInfo;

        public override void AddInnerStructures(uint param, BuildableGroup group, int z, int y, int x, VisualEffect effect)
        {
            SlopeSpaceFaces.AddInnerStructures(new SliverSlopeParams(param), this, this, group, z, y, x, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            SlopeSpaceFaces.AddOuterSurface(new SliverSlopeParams(param), this, this, group, z, y, x, face, effect, bump);
        }
    }
}
