using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;
using System;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class SliverSpaceViewModel : CellSpaceViewModel, IPlusCellSpace, ICellEdge
    {
        public SliverSpaceViewModel(SliverSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(SliverInfo.PlusMaterial, SliverInfo.PlusTiling);
            if (HasEdgeTiling)
                _EdgeTiling = Map.GetTileSet(CellEdgeInfo.EdgeMaterial, CellEdgeInfo.EdgeTiling);
        }

        private TileSetViewModel _PlusTiling { get; set; }
        private TileSetViewModel _EdgeTiling { get; set; }

        public SliverSpaceInfo SliverInfo
            => Info as SliverSpaceInfo;

        public override void AddInnerStructures(uint param, BuildableGroup group, int z, int y, int x, VisualEffect effect)
        {
            SliverSpaceFaces.AddInnerStructures(new SliverSlopeParams(param), this, this, group, z, y, x, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            SliverSpaceFaces.AddOuterSurface(new SliverSlopeParams(param), this, this, group, z, y, x, face, effect, bump);
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
            => new BuildableMaterial { Material = null, IsAlpha = false };

        public bool HasEdgeTiling
            => !string.IsNullOrEmpty(CellEdgeInfo.EdgeTiling);

        public bool HasPlusTiling
            => !string.IsNullOrEmpty(SliverInfo.PlusTiling);

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => SliverSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => SliverSpaceFaces.ShowFace(param, this, outwardFace);

        BuildableMaterial ICellEdge.GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (_EdgeTiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                        return new BuildableMaterial { Material = _EdgeTiling.ZPlusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    return new BuildableMaterial { Material = _EdgeTiling.ZMinusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                        return new BuildableMaterial { Material = _EdgeTiling.YPlusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    return new BuildableMaterial { Material = _EdgeTiling.YMinusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                        return new BuildableMaterial { Material = _EdgeTiling.XPlusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    return new BuildableMaterial { Material = _EdgeTiling.XMinusMaterial(effect), IsAlpha = _EdgeTiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }

        BuildableMaterial ICellEdge.GetOtherFaceMaterial(int index, VisualEffect effect)
        {
            if (_EdgeTiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            return new BuildableMaterial { Material = _EdgeTiling.WedgeMaterial(effect), IsAlpha = _EdgeTiling.GetWedgeAlpha() };
        }

        string ICellEdge.EdgeMaterial
            => SliverInfo.CellEdge.EdgeMaterial;

        string ICellEdge.EdgeTiling
            => SliverInfo.CellEdge.EdgeTiling;

        double ICellEdge.Width
            => SliverInfo.CellEdge.Width;

        BuildableMeshKey ICellEdge.GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _EdgeTiling?.TileSetInfo.BrushCollectionKey,
                BrushIndex = _EdgeTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };

        #region IPlusCellSpace Members

        public string PlusMaterialName => SliverInfo.PlusMaterial;
        public string PlusTilingName => SliverInfo.PlusTiling;
        public bool IsPlusGas => SliverInfo.IsPlusGas;
        public bool IsPlusSolid => SliverInfo.IsPlusSolid;
        public bool IsPlusLiquid => SliverInfo.IsPlusLiquid;
        public bool IsPlusInvisible => SliverInfo.IsPlusInvisible;

        public CellEdgeInfo CellEdgeInfo
            => SliverInfo.CellEdge;

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
