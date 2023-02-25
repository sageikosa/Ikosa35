using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class CornerSpaceViewModel : CellSpaceViewModel, IWedgeSpace
    {
        public CornerSpaceViewModel(CornerSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(CornerInfo.PlusMaterial, CornerInfo.PlusTiling);
        }

        public CornerSpaceInfo CornerInfo { get { return Info as CornerSpaceInfo; } }

        private TileSetViewModel _PlusTiling { get; set; }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            WedgeSpaceFaces.AddOuterSurface(param, this, group, z, y, x, face, effect, bump);
        }

        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            WedgeSpaceFaces.AddInnerStructures(param, this, addToGroup, z, y, x, effect);
        }

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (!HasPlusTiling)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                        return new BuildableMaterial { Material = _PlusTiling.ZPlusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    return new BuildableMaterial { Material = _PlusTiling.ZMinusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                        return new BuildableMaterial { Material = _PlusTiling.YPlusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    return new BuildableMaterial { Material = _PlusTiling.YMinusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                        return new BuildableMaterial { Material = _PlusTiling.XPlusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    return new BuildableMaterial { Material = _PlusTiling.XMinusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
        {
            if (!HasPlusTiling)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            return new BuildableMaterial { Material = _PlusTiling.WedgeMaterial(effect), IsAlpha = _PlusTiling.GetWedgeAlpha() };
        }

        public bool CornerStyle => !CornerInfo.IsWedge;
        public bool HasTiling => !string.IsNullOrEmpty(CornerInfo.Tiling);
        public bool HasPlusTiling => !string.IsNullOrEmpty(CornerInfo.PlusTiling);

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => WedgeSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => WedgeSpaceFaces.ShowFace(param, this, outwardFace);

        #region IWedgeSpace Members

        public string PlusMaterialName => CornerInfo.PlusMaterial;
        public string PlusTilingName => CornerInfo.PlusTiling;
        public double Offset1 => CornerInfo.Offset1;
        public double Offset2 => CornerInfo.Offset2;

        #endregion

        #region IPlusCellSpace Members

        public bool IsPlusGas => CornerInfo.IsPlusGas;
        public bool IsPlusSolid => CornerInfo.IsPlusSolid;
        public bool IsPlusLiquid => CornerInfo.IsPlusLiquid;
        public bool IsPlusInvisible => CornerInfo.IsPlusInvisible;

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _PlusTiling?.TileSetInfo.BrushCollectionKey,
                BrushIndex = _PlusTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };

        #endregion
    }
}