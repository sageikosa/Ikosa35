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
    public class SmallCylinderSpaceViewModel : CellSpaceViewModel, IWedgeSpace
    {
        public SmallCylinderSpaceViewModel(SmallCylinderSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(SmallCylinderSpaceInfo.PlusMaterial, SmallCylinderSpaceInfo.PlusTiling);
        }

        public SmallCylinderSpaceInfo SmallCylinderSpaceInfo => Info as SmallCylinderSpaceInfo;

        private TileSetViewModel _PlusTiling { get; set; }

        public bool HasPlusTiling => !string.IsNullOrEmpty(SmallCylinderSpaceInfo.PlusTiling);

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => false;

        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            CylinderSpaceFaces.AddSmallCylinder(param, this, addToGroup, effect);
        }

        public override void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            if (!IsPlusInvisible)
                CellSpaceFaces.AddPlusOuterSurface(param, this, buildable, z, y, x, face, effect, bump);
            CylinderSpaceFaces.AddSmallCylinderCap(param, this, buildable, z, y, x, face, effect, bump);
        }

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => WedgeSpaceFaces.ShowFace(param, this, outwardFace);

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

        public string PlusMaterialName => SmallCylinderSpaceInfo.PlusMaterial;
        public string PlusTilingName => SmallCylinderSpaceInfo.PlusTiling;
        public bool IsPlusGas => SmallCylinderSpaceInfo.IsPlusGas;
        public bool IsPlusSolid => SmallCylinderSpaceInfo.IsPlusSolid;
        public bool IsPlusLiquid => SmallCylinderSpaceInfo.IsPlusLiquid;
        public bool IsPlusInvisible => SmallCylinderSpaceInfo.IsPlusInvisible;

        #endregion

        public double Offset1 => 2.5d;
        public double Offset2 => 2.5d;
        public bool CornerStyle => true;
        public bool HasTiling => !string.IsNullOrEmpty(SmallCylinderSpaceInfo.Tiling);
    }
}
