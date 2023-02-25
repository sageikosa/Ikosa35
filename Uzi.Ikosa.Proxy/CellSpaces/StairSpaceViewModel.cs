using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class StairSpaceViewModel : CellSpaceViewModel, IPlusCellSpace
    {
        public StairSpaceViewModel(StairSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(StairInfo.PlusMaterial, StairInfo.PlusTiling);
        }

        public StairSpaceInfo StairInfo => Info as StairSpaceInfo;

        private TileSetViewModel _PlusTiling { get; set; }

        #region protected IEnumerable<CellStructureInfo> Components(uint param)
        protected IEnumerable<CellStructureInfo> Components(uint param)
        {
            var _climbOpen = StairSpaceFaces.GetClimbOpening(param);
            var _travelOpen = StairSpaceFaces.GetTravelOpening(param);

            #region climb base
            if (!_climbOpen.IsLowFace())
            {
                var _sliver = new SliverSpaceInfo
                {
                    CellMaterial = StairInfo.CellMaterial,
                    Tiling = StairInfo.Tiling,
                    IsGas = StairInfo.IsGas,
                    IsInvisible = StairInfo.IsInvisible,
                    IsLiquid = StairInfo.IsLiquid,
                    IsSolid = StairInfo.IsSolid,
                    PlusMaterial = StairInfo.PlusMaterial,
                    PlusTiling = StairInfo.PlusTiling,
                    IsPlusGas = StairInfo.IsPlusGas,
                    IsPlusInvisible = StairInfo.IsPlusInvisible,
                    IsPlusLiquid = StairInfo.IsPlusLiquid,
                    IsPlusSolid = StairInfo.IsPlusSolid,
                    CellEdge = new CellEdgeInfo()
                };
                yield return new CellStructureInfo
                {
                    CellSpace = new SliverSpaceViewModel(_sliver, Map),
                    ParamData = (new SliverSlopeParams { Axis = _climbOpen.GetAxis(), Flip = false, OffsetUnits = 0 }).Value
                };
            }
            else
            {
                var _sliver = new SliverSpaceInfo
                {
                    CellMaterial = StairInfo.PlusMaterial,
                    Tiling = StairInfo.PlusTiling,
                    IsGas = StairInfo.IsPlusGas,
                    IsInvisible = StairInfo.IsPlusInvisible,
                    IsLiquid = StairInfo.IsPlusLiquid,
                    IsSolid = StairInfo.IsPlusSolid,
                    PlusMaterial = StairInfo.CellMaterial,
                    PlusTiling = StairInfo.Tiling,
                    IsPlusGas = StairInfo.IsGas,
                    IsPlusInvisible = StairInfo.IsInvisible,
                    IsPlusLiquid = StairInfo.IsLiquid,
                    IsPlusSolid = StairInfo.IsSolid,
                    CellEdge = new CellEdgeInfo()
                };
                yield return new CellStructureInfo
                {
                    CellSpace = new SliverSpaceViewModel(_sliver, Map),
                    ParamData = (new SliverSlopeParams { Axis = _climbOpen.GetAxis(), Flip = false, OffsetUnits = 60 }).Value
                };
            }
            #endregion

            #region travel through base
            if (!_travelOpen.IsLowFace())
            {
                var _sliver = new SliverSpaceInfo
                {
                    CellMaterial = StairInfo.CellMaterial,
                    Tiling = StairInfo.Tiling,
                    IsGas = StairInfo.IsGas,
                    IsInvisible = StairInfo.IsInvisible,
                    IsLiquid = StairInfo.IsLiquid,
                    IsSolid = StairInfo.IsSolid,
                    PlusMaterial = StairInfo.PlusMaterial,
                    PlusTiling = StairInfo.PlusTiling,
                    IsPlusGas = StairInfo.IsPlusGas,
                    IsPlusInvisible = StairInfo.IsPlusInvisible,
                    IsPlusLiquid = StairInfo.IsPlusLiquid,
                    IsPlusSolid = StairInfo.IsPlusSolid,
                    CellEdge = new CellEdgeInfo()
                };
                yield return new CellStructureInfo
                {
                    CellSpace = new SliverSpaceViewModel(_sliver, Map),
                    ParamData = (new SliverSlopeParams { Axis = _travelOpen.GetAxis(), Flip = false, OffsetUnits = 1 }).Value
                };
            }
            else
            {
                var _sliver = new SliverSpaceInfo
                {
                    CellMaterial = StairInfo.PlusMaterial,
                    Tiling = StairInfo.PlusTiling,
                    IsGas = StairInfo.IsPlusGas,
                    IsInvisible = StairInfo.IsPlusInvisible,
                    IsLiquid = StairInfo.IsPlusLiquid,
                    IsSolid = StairInfo.IsPlusSolid,
                    PlusMaterial = StairInfo.CellMaterial,
                    PlusTiling = StairInfo.Tiling,
                    IsPlusGas = StairInfo.IsGas,
                    IsPlusInvisible = StairInfo.IsInvisible,
                    IsPlusLiquid = StairInfo.IsLiquid,
                    IsPlusSolid = StairInfo.IsSolid,
                    CellEdge = new CellEdgeInfo()
                };
                yield return new CellStructureInfo
                {
                    CellSpace = new SliverSpaceViewModel(_sliver, Map),
                    ParamData = (new SliverSlopeParams { Axis = _travelOpen.GetAxis(), Flip = false, OffsetUnits = 59 }).Value
                };
            }
            #endregion

            uint _wedgeParam = StairSpaceFaces.WedgeParallelParam(_climbOpen, _travelOpen);
            double _stepSize = 5d / (double)StairInfo.Steps;
            double _secondary = 5d - _stepSize;
            double _primary = _stepSize;
            for (int _sx = 0; _sx < StairInfo.Steps - 1; _sx++)
            {
                var _wedge = new CornerSpaceInfo
                {
                    CellMaterial = StairInfo.CellMaterial,
                    Tiling = StairInfo.Tiling,
                    IsGas = StairInfo.IsGas,
                    IsInvisible = StairInfo.IsInvisible,
                    IsLiquid = StairInfo.IsLiquid,
                    IsSolid = StairInfo.IsSolid,
                    PlusMaterial = StairInfo.PlusMaterial,
                    PlusTiling = StairInfo.PlusTiling,
                    IsPlusGas = StairInfo.IsPlusGas,
                    IsPlusInvisible = StairInfo.IsPlusInvisible,
                    IsPlusLiquid = StairInfo.IsPlusLiquid,
                    IsPlusSolid = StairInfo.IsPlusSolid,
                    Offset1 = _primary,
                    Offset2 = _secondary
                };
                yield return new CellStructureInfo
                {
                    CellSpace = new CornerSpaceViewModel(_wedge, Map),
                    ParamData = _wedgeParam
                };
                _primary += _stepSize;
                _secondary -= _stepSize;
            }
            yield break;
        }
        #endregion

        public override void AddInnerStructures(uint param, BuildableGroup group, int z, int y, int x, VisualEffect effect)
        {
            foreach (var _component in Components(param))
                _component.AddInnerStructures(group, z, y, x, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            foreach (var _component in Components(param))
                _component.AddOuterSurface(group, z, y, x, face, effect, bump, currentRegion);
        }

        public bool HasPlusTiling => !string.IsNullOrEmpty(StairInfo.PlusTiling);

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => StairSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => StairSpaceFaces.ShowFace(param, this, outwardFace);

        #region IPlusCellSpace Members

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect)
        {
            if (!HasPlusTiling)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (axis)
            {
                case Axis.Z:
                    if (isPlusFace)
                        return new BuildableMaterial { Material = _PlusTiling.ZPlusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    return new BuildableMaterial { Material = _PlusTiling.ZMinusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlusFace)
                        return new BuildableMaterial { Material = _PlusTiling.YPlusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    return new BuildableMaterial { Material = _PlusTiling.YMinusMaterial(effect), IsAlpha = _PlusTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlusFace)
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

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _PlusTiling?.TileSetInfo.BrushCollectionKey,
                BrushIndex = _PlusTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };

        public string PlusMaterialName => StairInfo.PlusMaterial;
        public string PlusTilingName => StairInfo.PlusTiling;
        public bool IsPlusGas => StairInfo.IsPlusGas;
        public bool IsPlusSolid => StairInfo.IsPlusSolid;
        public bool IsPlusLiquid => StairInfo.IsPlusLiquid;
        public bool IsPlusInvisible => StairInfo.IsPlusInvisible;

        #endregion
    }
}
