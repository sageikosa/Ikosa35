using System;
using System.Collections.Generic;
using System.ComponentModel;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Uzi.Packaging;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class BasePanel : INotifyPropertyChanged, IBasePanel
    {
        public BasePanel(string name, CellMaterial material, TileSet tiling, double thickness)
        {
            _Material = material;
            _Tiling = tiling;
            _Thick = CanSetThickness(thickness) ? thickness : 0.25d;
            _Name = name;
            _Dangling = null;
            _Base = null;
            _Ledge = null;
        }

        #region private data
        private CellMaterial _Material;
        private TileSet _Tiling;
        private double _Thick;
        private string _Name;
        private int? _Dangling;
        private int? _Base;
        private int? _Ledge;
        #endregion

        // TODO: vary these by which side if facing the creature...

        public int? Dangling
        {
            get { return _Dangling; }
            set { _Dangling = value; }
        }

        public int? Base
        {
            get { return _Base; }
            set { _Base = value; }
        }

        public int? Ledge
        {
            get { return _Ledge; }
            set { _Ledge = value; }
        }

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public virtual LocalMap Map => (Material?.LocalMap);

        #region public CellMaterial Material { get; set; }
        public CellMaterial Material
        {
            get => _Material;
            set
            {
                if (_Material != value)
                {
                    _Material = value;
                    DoPropertyChanged(nameof(Material));
                }
            }
        }
        #endregion

        #region public TileSet Tiling { get; set; }
        public TileSet Tiling
        {
            get => _Tiling;
            set
            {
                if (_Tiling != value)
                {
                    _Tiling = value;
                    DoPropertyChanged(nameof(Tiling));
                }
            }
        }
        #endregion

        /// <summary>Maximum thickness is 0.25</summary>
        protected virtual bool CanSetThickness(double value)
            => (value >= 0) && (value <= 0.25d);

        public virtual double AverageThickness => Thickness;

        public double Thickness
        {
            get => _Thick;
            set
            {
                if (CanSetThickness(value))
                {
                    _Thick = value;
                    DoPropertyChanged(nameof(Thickness));
                }
            }
        }

        public abstract bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces);
        public abstract HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels);

        // TODO: helpers for panel thickness and orientation adjustments
        // TODO: intersectable planes

        protected abstract bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2);
        protected abstract bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors);

        #region public bool BlocksPath(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        /// <summary>True if a segment of the line passes through the panel, and the material blocks effect</summary>
        public bool BlocksPath(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            return Material.BlocksEffect && IntersectsPanel(param, panelFace, z, y, x, p1, p2);
        }
        #endregion

        #region public bool BlocksPath(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        /// <summary>True if a segment of the line passes through the panel, and the material blocks effect</summary>
        public bool BlocksPath(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            return Material.BlocksEffect && IntersectsPanel(param, z, y, x, p1, p2, interiors);
        }
        #endregion

        /// <summary>Length of panel material transit</summary>
        protected virtual double TransitLength(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _vector = TransitSegment(param, panelFace, z, y, x, p1, p2);
            if (_vector.HasValue)
                return _vector.Value.Vector.Length;
            return 0d;
        }

        /// <summary>Length of panel material transit</summary>
        protected virtual double TransitLength(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            var _vector = TransitSegment(param, z, y, x, p1, p2, interiors);
            if (_vector.HasValue)
                return _vector.Value.Vector.Length;
            return 0d;
        }

        protected abstract Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2);
        protected abstract Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors);

        #region public bool BlocksDetect(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        /// <summary>True is a segment of the line passes through the panel and the segment is thick enough to block detection</summary>
        public bool BlocksDetect(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            return TransitLength(param, panelFace, z, y, x, p1, p2) >= Material.DetectBlockingThickness;
        }
        #endregion

        #region public bool BlocksDetect(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        /// <summary>True is a segment of the line passes through the panel and the segment is thick enough to block detection</summary>
        public bool BlocksDetect(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            return TransitLength(param, z, y, x, p1, p2, interiors) >= Material.DetectBlockingThickness;
        }
        #endregion

        public abstract void AddOuterSurface(PanelParams param, BuildableGroup pair, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels);

        /// <summary>Returns true if panel completely blocks exposure to the sideFace</summary>
        public abstract bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace);

        #region IBasePanel Members

        public virtual bool IsGas => Material is GasCellMaterial;
        public virtual bool IsLiquid => Material is LiquidCellMaterial;
        public virtual bool IsInvisible => IsGas && (Material as GasCellMaterial).IsInvisible;

        #region public bool OutwardVisible(int towardsZ, int towardsY, int towardsX)
        public bool OutwardVisible(int towardsZ, int towardsY, int towardsX)
        {
            var _neighbor = Material.LocalMap[towardsZ, towardsY, towardsX, null];
            if (_neighbor == null)
            {
                // edge of map doesn't face anywhere
                return false;
            }

            if (_neighbor.Template.GetType() != typeof(CellSpace))
            {
                // facing something of unusual shape, so best to display...
                return true;
            }

            // uniform fill...
            var _neighborSpace = _neighbor.Template as CellSpace;
            var _neighborMaterial = _neighborSpace.CellMaterial;
            if (_neighborMaterial is SolidCellMaterial _solid)
            {
                // facing a solid, cannot see through solids
                return false;
            }

            if ((_neighborMaterial != Material) || (_neighborSpace.Tiling != Tiling))
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
            var _neighbor = Material.LocalMap[towardsZ, towardsY, towardsX, null];
            if (_neighbor == null)
            {
                // edge of map cuts a boundary
                return true;
            }

            if (_neighbor.Template.GetType() != typeof(CellSpace))
            {
                // facing something of unusual shape, so best to display...
                return true;
            }

            var _neighborSpace = _neighbor.Template as CellSpace;
            var _neighborMaterial = _neighborSpace.CellMaterial;
            if ((_neighborMaterial != _Material) || (_neighborSpace.Tiling != Tiling))
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
            if (Tiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (side)
            {
                case SideIndex.Top:
                    return new BuildableMaterial { Material = Tiling.TopSideMaterial(effect), IsAlpha = Tiling.TopSideAlpha };
                case SideIndex.Bottom:
                    return new BuildableMaterial { Material = Tiling.BottomSideMaterial(effect), IsAlpha = Tiling.BottomSideAlpha };
                case SideIndex.Front:
                    return new BuildableMaterial { Material = Tiling.InsideMaterial(effect), IsAlpha = Tiling.InsideAlpha };
                case SideIndex.Back:
                    return new BuildableMaterial { Material = Tiling.OutsideMaterial(effect), IsAlpha = Tiling.OutsideAlpha };
                case SideIndex.Left:
                    return new BuildableMaterial { Material = Tiling.LeftSideMaterial(effect), IsAlpha = Tiling.LeftSideAlpha };
                case SideIndex.Right:
                default:
                    return new BuildableMaterial { Material = Tiling.RightSideMaterial(effect), IsAlpha = Tiling.RightSideAlpha };
            }
        }
        #endregion

        #region public BuildableMaterial GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        public BuildableMaterial GetAnchorFaceMaterial(AnchorFace face, VisualEffect effect)
        {
            if (Tiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (face)
            {
                case AnchorFace.ZHigh:
                    return new BuildableMaterial { Material = Tiling.ZPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
                case AnchorFace.ZLow:
                    return new BuildableMaterial { Material = Tiling.ZMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
                case AnchorFace.YHigh:
                    return new BuildableMaterial { Material = Tiling.YPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
                case AnchorFace.YLow:
                    return new BuildableMaterial { Material = Tiling.YMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
                case AnchorFace.XHigh:
                    return new BuildableMaterial { Material = Tiling.XPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
                case AnchorFace.XLow:
                default:
                    return new BuildableMaterial { Material = Tiling.XMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(face) };
            }
        }
        #endregion

        #region public BuildableMaterial GetWedgeMaterial(VisualEffect effect)
        public BuildableMaterial GetWedgeMaterial(VisualEffect effect)
        {
            if (Tiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            return new BuildableMaterial { Material = Tiling.WedgeMaterial(effect), IsAlpha = Tiling.GetWedgeAlpha() };
        }
        #endregion

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = (Tiling != null) ? Tiling.BrushCollectionKey : string.Empty,
                BrushIndex = (Tiling != null) ? Tiling.GetAnchorFaceMaterialIndex(face) : 0
            };
        }

        public BuildableMeshKey GetBuildableMeshKey(SideIndex side, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = (Tiling != null) ? Tiling.BrushCollectionKey : string.Empty,
                BrushIndex = (Tiling != null) ? Tiling.GetSideIndexMaterialIndex(side) : 0
            };
        }

        #endregion

        #region ICorePart Members

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public IEnumerable<ICorePart> Relationships { get { yield break; } }
        public string TypeName => GetType().FullName;

        #endregion

        #region IBasePanel Members

        public string MaterialName => Material?.Name ?? string.Empty;
        public string TilingName => Tiling?.Name ?? string.Empty;
        public bool IsSolid => Material is SolidCellMaterial;

        #endregion
    }
}