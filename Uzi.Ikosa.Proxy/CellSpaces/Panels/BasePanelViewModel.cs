using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public abstract partial class BasePanelViewModel : IBasePanel
    {
        public BasePanelViewModel(BasePanelInfo info, LocalMapInfo map)
        {
            _Info = info;
            Map = map;
            _TileSet = Map.GetTileSet(Info.Material, Info.Tiling);
        }

        public virtual LocalMapInfo Map { get; set; }
        public BasePanelInfo Info { get { return _Info; } }

        private BasePanelInfo _Info;
        private TileSetViewModel _TileSet { get; set; }

        #region IBasePanel Members

        #region public bool OutwardVisible(int towardsZ, int towardsY, int towardsX)
        public bool OutwardVisible(int towardsZ, int towardsY, int towardsX)
        {
            var _neighbor = Map[towardsZ, towardsY, towardsX];
            if (_neighbor.CellSpace == null)
            {
                // edge of map doesn't face anywhere
                return false;
            }

            if (_neighbor.CellSpace.GetType() != typeof(CellSpaceViewModel))
            {
                // facing something of unusual shape, so best to display...
                return true;
            }

            // uniform fill...
            if (_neighbor.CellSpace.IsSolid)
            {
                // facing a solid, cannot see through solids
                return false;
            }

            if ((_neighbor.CellSpace.Info.CellMaterial != Info.Material) || (_neighbor.CellSpace.Info.Tiling != Info.Tiling))
            {
                // bumped up against something that doesn't match us
                return true;
            }

            // some uniform non-solid that matches our material and tiling exactly...
            return false;
        }
        #endregion

        #region public bool InwardVisible(int towardsZ, int towardsY, int towardsX)
        public bool InwardVisible(int towardsZ, int towardsY, int towardsX)
        {
            var _neighbor = Map[towardsZ, towardsY, towardsX];
            if (_neighbor.CellSpace == null)
            {
                // edge of map cuts a boundary
                return true;
            }

            if (_neighbor.CellSpace.GetType() != typeof(CellSpaceViewModel))
            {
                // facing something of unusual shape, so best to display...
                return true;
            }

            if ((_neighbor.CellSpace.Info.CellMaterial != Info.Material) || (_neighbor.CellSpace.Info.Tiling != Info.Tiling))
            {
                // non-contiguous with the liquid/gas medium
                return true;
            }

            // facing an identical cell
            return false;
        }
        #endregion

        #region public BuildableMaterial GetSideFaceMaterial(SideIndex side, VisualEffect effect)
        public BuildableMaterial GetSideFaceMaterial(SideIndex side, VisualEffect effect)
        {
            if (string.IsNullOrEmpty(Info.Tiling))
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (side)
            {
                case SideIndex.Top:
                    return new BuildableMaterial { Material = _TileSet.TopSideMaterial(effect), IsAlpha = _TileSet.TopSideAlpha };
                case SideIndex.Bottom:
                    return new BuildableMaterial { Material = _TileSet.BottomSideMaterial(effect), IsAlpha = _TileSet.BottomSideAlpha };
                case SideIndex.Front:
                    return new BuildableMaterial { Material = _TileSet.InsideMaterial(effect), IsAlpha = _TileSet.InsideAlpha };
                case SideIndex.Back:
                    return new BuildableMaterial { Material = _TileSet.OutsideMaterial(effect), IsAlpha = _TileSet.OutsideAlpha };
                case SideIndex.Left:
                    return new BuildableMaterial { Material = _TileSet.LeftSideMaterial(effect), IsAlpha = _TileSet.LeftSideAlpha };
                case SideIndex.Right:
                default:
                    return new BuildableMaterial { Material = _TileSet.RightSideMaterial(effect), IsAlpha = _TileSet.RightSideAlpha };
            }
        }
        #endregion

        #region public BuildableMaterial GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        public BuildableMaterial GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        {
            if (string.IsNullOrEmpty(Info.Tiling))
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (face)
            {
                case AnchorFace.ZHigh:
                    return new BuildableMaterial { Material = _TileSet.ZPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
                case AnchorFace.ZLow:
                    return new BuildableMaterial { Material = _TileSet.ZMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
                case AnchorFace.YHigh:
                    return new BuildableMaterial { Material = _TileSet.YPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
                case AnchorFace.YLow:
                    return new BuildableMaterial { Material = _TileSet.YMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
                case AnchorFace.XHigh:
                    return new BuildableMaterial { Material = _TileSet.XPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
                case AnchorFace.XLow:
                default:
                    return new BuildableMaterial { Material = _TileSet.XMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(face) };
            }
        }
        #endregion

        #region public BuildableMaterial GetWedgeMaterial(VisualEffect effect)
        public BuildableMaterial GetWedgeMaterial(VisualEffect effect)
        {
            if (Info.Tiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            return new BuildableMaterial { Material = _TileSet.WedgeMaterial(effect), IsAlpha = _TileSet.GetWedgeAlpha() };
        }
        #endregion

        public abstract void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels);

        public abstract bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace);

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _TileSet.TileSetInfo.BrushCollectionKey,
                BrushIndex = _TileSet.GetAnchorFaceMaterialIndex(face)
            };

        public BuildableMeshKey GetBuildableMeshKey(SideIndex side, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _TileSet?.TileSetInfo.BrushCollectionKey ?? string.Empty,
                BrushIndex = _TileSet?.GetSideIndexMaterialIndex(side) ?? 0
            };
        }

        #endregion

        #region ICorePart Members

        public IEnumerable<Packaging.ICorePart> Relationships
        {
            get { yield break; }
        }

        public string TypeName
        {
            get { return this.GetType().FullName; }
        }

        #endregion

        #region IBasePanel Members

        public bool IsGas
        {
            get { return Info.IsGas; }
        }

        public bool IsSolid
        {
            get { return Info.IsSolid; }
        }

        public bool IsLiquid
        {
            get { return Info.IsLiquid; }
        }

        public bool IsInvisible
        {
            get { return Info.IsInvisible; }
        }

        public double Thickness
        {
            get { return Info.Thickness; }
        }

        public string MaterialName
        {
            get { return Info.Material; }
        }

        public string TilingName
        {
            get { return Info.Tiling; }
        }

        #endregion

        #region ICorePart Members

        public string Name
        {
            get { return Info.Name; }
        }

        #endregion
    }
}
