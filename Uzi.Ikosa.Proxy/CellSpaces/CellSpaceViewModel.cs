using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class CellSpaceViewModel : ICellSpace
    {
        public static CellSpaceViewModel GetViewModel(CellSpaceInfo space, LocalMapInfo map)
        {
            if (space is SlopeSpaceInfo)
                return new SlopeSpaceViewModel(space as SlopeSpaceInfo, map);
            else if (space is SliverSpaceInfo)
                return new SliverSpaceViewModel(space as SliverSpaceInfo, map);

            else if (space is StairSpaceInfo)
                return new StairSpaceViewModel(space as StairSpaceInfo, map);
            else if (space is LFrameSpaceInfo)
                return new LFrameSpaceViewModel(space as LFrameSpaceInfo, map);
            else if (space is PanelSpaceInfo)
                return new PanelSpaceViewModel(space as PanelSpaceInfo, map);
            else if (space is SmallCylinderSpaceInfo)
                return new SmallCylinderSpaceViewModel(space as SmallCylinderSpaceInfo, map);
            else if (space is CornerSpaceInfo)
                return new CornerSpaceViewModel(space as CornerSpaceInfo, map);
            else if (space is CylinderSpaceInfo)
                return new CylinderSpaceViewModel(space as CylinderSpaceInfo, map);
            else
                return new CellSpaceViewModel(space, map);
        }

        public CellSpaceViewModel(CellSpaceInfo info, LocalMapInfo map)
        {
            Info = info;
            Map = map;
            _TileSet = Map.GetTileSet(this.Info.CellMaterial, this.Info.Tiling);
        }

        public virtual LocalMapInfo Map { get; set; }

        public CellSpaceInfo Info { get; private set; }
        private TileSetViewModel _TileSet { get; set; }

        // TODO: change addToGroup into something that allows more abstract and advanced services for drawing
        public virtual void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, IGeometricRegion currentRegion)
        {
            CellSpaceFaces.AddOuterSurface(param, this, group, z, y, x, face, effect, bump);
        }

        /// <summary>Generates Model3DGroup for any part of the cell that is not flush to the surface</summary>
        public virtual void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            return;
        }

        #region public virtual bool OutwardVisible(uint param, int z, int y, int x)
        public virtual bool OutwardVisible(uint param, int z, int y, int x)
        {
            var _neighbor = Map[z, y, x];
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
            if (_neighbor.CellSpace.Info.IsSolid)
            {
                // facing a solid, cannot see through solids
                return false;
            }

            if ((_neighbor.CellSpace.Info.CellMaterial != Info.CellMaterial) || (_neighbor.CellSpace.Info.Tiling != Info.Tiling))
            {
                // bumped up against something that doesn't match us
                return true;
            }

            // some uniform non-solid that matches our material and tiling exactly...
            return false;
        }
        #endregion

        #region public virtual bool InwardVisible(uint param, int z, int y, int x)
        public virtual bool InwardVisible(uint param, int z, int y, int x)
        {
            var _neighbor = Map[z, y, x];
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

            if ((_neighbor.CellSpace.Info.CellMaterial != Info.CellMaterial) || (_neighbor.CellSpace.Info.Tiling != Info.Tiling))
            {
                // non-contiguous with the liquid/gas medium
                return true;
            }

            // facing an identical cell
            return false;
        }
        #endregion

        #region public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (_TileSet == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                        return new BuildableMaterial { Material = _TileSet.ZPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    return new BuildableMaterial { Material = _TileSet.ZMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                        return new BuildableMaterial { Material = _TileSet.YPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    return new BuildableMaterial { Material = _TileSet.YMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                        return new BuildableMaterial { Material = _TileSet.XPlusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    return new BuildableMaterial { Material = _TileSet.XMinusMaterial(effect), IsAlpha = _TileSet.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }
        #endregion

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
        {
            if (_TileSet == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            return new BuildableMaterial { Material = _TileSet.WedgeMaterial(effect), IsAlpha = _TileSet.GetWedgeAlpha() };
        }

        #region ICellSpace Members

        public virtual bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentGroup)
            => Map[z, y, x, neighborFace, currentGroup].OccludesFace(neighborFace.ReverseFace());

        public virtual bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => Info.IsSolid
                ? true
                : (Info.IsInvisible ? (bool?)false : null);

        public virtual bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => false;

        public virtual bool? OccludesFace(uint param, AnchorFace outwardFace)
            => Info.IsSolid
                ? true
                : (Info.IsInvisible ? (bool?)false : null);

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = _TileSet?.TileSetInfo.BrushCollectionKey,
                BrushIndex = _TileSet?.GetAnchorFaceMaterialIndex(face) ?? 0
            };

        public uint Index => Info.Index;
        public string Name => Info.Name;
        public string CellMaterialName => Info.CellMaterial;
        public string TilingName => Info.Tiling;
        public bool IsGas => Info.IsGas;
        public bool IsSolid => Info.IsSolid;
        public bool IsLiquid => Info.IsLiquid;
        public bool IsInvisible => Info.IsInvisible;

        #endregion
    }
}
