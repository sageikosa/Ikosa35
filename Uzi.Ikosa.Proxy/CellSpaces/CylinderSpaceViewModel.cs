using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class CylinderSpaceViewModel : CellSpaceViewModel, IPlusCellSpace
    {
        public CylinderSpaceViewModel(CylinderSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(CylinderSpaceInfo.PlusMaterial, CylinderSpaceInfo.PlusTiling);
        }

        public CylinderSpaceInfo CylinderSpaceInfo => Info as CylinderSpaceInfo;

        private TileSetViewModel _PlusTiling { get; set; }

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            CylinderSpaceFaces.AddCylindricalQuarterFace(param, this, group, z, y, x, face, effect, bump);
            if (!IsPlusInvisible)
                CellSpaceFaces.AddPlusOuterSurface(param, this, group, z, y, x, face, effect, bump);
            // TODO: pediment? cornice?
        }

        public bool HasPlusTiling => !string.IsNullOrEmpty(CylinderSpaceInfo.PlusTiling);

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => false;

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => true;

        public override bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => outwardFace.GetAxis() != new CylinderParams(param).AnchorFace.GetAxis();

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

        public string PlusMaterialName => CylinderSpaceInfo.PlusMaterial;
        public string PlusTilingName => CylinderSpaceInfo.PlusTiling;
        public bool IsPlusGas => CylinderSpaceInfo.IsPlusGas;
        public bool IsPlusSolid => CylinderSpaceInfo.IsPlusSolid;
        public bool IsPlusLiquid => CylinderSpaceInfo.IsPlusLiquid;
        public bool IsPlusInvisible => CylinderSpaceInfo.IsPlusInvisible;

        #endregion
    }
}
