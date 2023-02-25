using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class LFrameSpaceViewModel : CellSpaceViewModel, IPlusCellSpace
    {
        public LFrameSpaceViewModel(LFrameSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            if (HasPlusTiling)
                _PlusTiling = Map.GetTileSet(LFrameInfo.PlusMaterial, LFrameInfo.PlusTiling);
        }

        public LFrameSpaceInfo LFrameInfo { get { return Info as LFrameSpaceInfo; } }

        private TileSetViewModel _PlusTiling { get; set; }

        #region protected IEnumerable<CellStructureInfo> Components(uint param)
        protected IEnumerable<CellStructureInfo> Components(uint param)
        {
            var _thickFace = LFrameSpaceFaces.GetThickFace(param);
            var _frame1Face = LFrameSpaceFaces.GetFrame1Face(param);
            var _frame2Face = LFrameSpaceFaces.GetFrame2Face(param);
            CornerSpaceInfo _frame1 = null;
            CornerSpaceInfo _frame2 = null;

            Action<Axis> _create = (Axis axis) =>
            {
                if (_frame1Face.GetAxis() == axis)
                {
                    _frame1 = new CornerSpaceInfo
                    {
                        CellMaterial = LFrameInfo.CellMaterial,
                        Tiling = LFrameInfo.Tiling,
                        PlusMaterial = LFrameInfo.PlusMaterial,
                        PlusTiling = LFrameInfo.PlusTiling,
                        Offset1 = LFrameInfo.Width1,
                        Offset2 = LFrameInfo.Thickness
                    };
                    _frame2 = new CornerSpaceInfo
                    {
                        CellMaterial = LFrameInfo.CellMaterial,
                        Tiling = LFrameInfo.Tiling,
                        PlusMaterial = LFrameInfo.PlusMaterial,
                        PlusTiling = LFrameInfo.PlusTiling,
                        Offset1 = LFrameInfo.Thickness,
                        Offset2 = LFrameInfo.Width2
                    };
                }
                else
                {
                    _frame1 = new CornerSpaceInfo
                    {
                        CellMaterial = LFrameInfo.CellMaterial,
                        Tiling = LFrameInfo.Tiling,
                        PlusMaterial = LFrameInfo.PlusMaterial,
                        PlusTiling = LFrameInfo.PlusTiling,
                        Offset1 = LFrameInfo.Thickness,
                        Offset2 = LFrameInfo.Width1
                    };
                    _frame2 = new CornerSpaceInfo
                    {
                        CellMaterial = LFrameInfo.CellMaterial,
                        Tiling = LFrameInfo.Tiling,
                        PlusMaterial = LFrameInfo.PlusMaterial,
                        PlusTiling = LFrameInfo.PlusTiling,
                        Offset1 = LFrameInfo.Width2,
                        Offset2 = LFrameInfo.Thickness
                    };
                }
            };

            switch (_thickFace.GetAxis())
            {
                case Axis.X:
                    _create(Axis.Z);
                    break;

                case Axis.Y:
                    _create(Axis.X);
                    break;

                case Axis.Z:
                    _create(Axis.Y);
                    break;
            }

            yield return new CellStructureInfo
            {
                CellSpace = new CornerSpaceViewModel(_frame1, Map),
                ParamData = StairSpaceFaces.WedgeParallelParam(_thickFace.ReverseFace(), _frame1Face.ReverseFace())
            };

            yield return new CellStructureInfo
            {
                CellSpace = new CornerSpaceViewModel(_frame2, Map),
                ParamData = StairSpaceFaces.WedgeParallelParam(_thickFace.ReverseFace(), _frame2Face.ReverseFace())
            };
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

        public bool HasPlusTiling { get { return !string.IsNullOrEmpty(LFrameInfo.PlusTiling); } }

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
        {
            return LFrameSpaceFaces.OccludesFace(param, this, outwardFace);
        }

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
        {
            return LFrameSpaceFaces.ShowFace(param, this, outwardFace);
        }

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

        public string PlusMaterialName => LFrameInfo.PlusMaterial;
        public string PlusTilingName => LFrameInfo.PlusTiling;
        public bool IsPlusGas => LFrameInfo.IsPlusGas;
        public bool IsPlusSolid => LFrameInfo.IsPlusSolid;
        public bool IsPlusLiquid => LFrameInfo.IsPlusLiquid;
        public bool IsPlusInvisible => LFrameInfo.IsPlusInvisible;

        #endregion
    }
}
