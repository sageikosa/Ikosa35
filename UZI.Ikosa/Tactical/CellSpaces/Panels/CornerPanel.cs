using System;
using System.Collections.Generic;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CornerPanel : BaseNaturalPanel, ICornerPanel
    {
        public CornerPanel(string name, SolidCellMaterial material, TileSet tiling, double thickness, double offset)
            : base(name, material, tiling, thickness)
        {
            _Offset = offset;
        }

        #region data
        private double _Offset;
        #endregion

        #region public double Offset { get; set; }
        public double Offset
        {
            get => _Offset;
            set
            {
                _Offset = value;
                DoPropertyChanged(nameof(Offset));
            }
        }
        #endregion

        #region public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> _faces)
        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> _faces)
        {
            if (movement.CanMoveThrough(Material))
                return false;

            // cornered edge face
            var _snap = param.GetPanelEdge(panelFace);
            var _edgeFace = panelFace.GetSnappedFace(_snap);

            // only blocked if _faces represents shared edge, or shared corners
            return _faces.Contains(panelFace) && _faces.Contains(_edgeFace);
        }
        #endregion

        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        {
            if (movement.CanMoveThrough(Material))
                return new HedralGrip(false);

            // offset from corner divided by maximum extent
            return new HedralGrip(panelFace.GetAxis(), panelFace.GetSnappedFace(param.GetPanelEdge(panelFace)), Offset);
        }

        #region private OrthoRectangularPrism GetOrthoRectangularPrism(PanelParams param, AnchorFace panelFace, int z, int y, int x)
        private OrthoRectangularPrism GetOrthoRectangularPrism(PanelParams param, AnchorFace panelFace, int z, int y, int x)
        {
            var _panelAxis = panelFace.GetAxis();
            var _snapped = panelFace.GetSnappedFace(param.GetPanelEdge(panelFace));

            // position
            var _pt = new Point3D(x * 5d, y * 5d, z * 5d);

            // offset for panel face axis, and snap axis when each is not it's axial low face
            Func<Axis, double> _offset =
                (axis) => (_panelAxis == axis)
                    ? (!panelFace.IsLowFace() ? 5d - Thickness : 0d)
                    : ((!_snapped.IsLowFace() && (_snapped.GetAxis() == axis)) ? 5d - Offset : 0d);
            _pt.Offset(_offset(Axis.X), _offset(Axis.Y), _offset(Axis.Z));

            // thickness, snap, or full extent
            Func<Axis, double> _sizePart =
                (axis) => (_panelAxis == axis)
                    ? Thickness
                    : (_snapped.GetAxis() == axis ? Offset : 5);
            var _size = new Vector3D(_sizePart(Axis.X), _sizePart(Axis.Y), _sizePart(Axis.Z));

            // prism and transit vector
            var _prism = new OrthoRectangularPrism(_pt, _size);
            return _prism;
        }
        #endregion

        #region protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _prism = GetOrthoRectangularPrism(param, panelFace, z, y, x);
            return _prism.TransitSegment(p1, p2);
        }
        #endregion

        #region protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            return null;
        }
        #endregion

        protected override bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _prism = GetOrthoRectangularPrism(param, panelFace, z, y, x);
            return _prism.Intersects(p1, p2);
        }

        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => false;

        public override void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            // TODO: determine visiblity?
            PanelSpaceFaces.AddInnerCornerPanel(addTogroup, panelFace, Thickness, param.GetPanelEdge(panelFace), Offset, effect, this);
        }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            if (param.GetPanelType(panelFace) == PanelType.MaskedCorner)
            {
                if (panelFace == visibleFace)
                    PanelSpaceFaces.AddOuterNormalPanel(group, new CellPosition(z, y, x), panelFace, effect, this);
                else
                    PanelSpaceFaces.AddOuterNormalPanel(group, z, y, x, panelFace, visibleFace, Thickness, effect, this);
            }
            else
                PanelSpaceFaces.AddOuterCornerPanel(group, z, y, x, panelFace, visibleFace, Thickness, param.GetPanelEdge(panelFace), Offset, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                if (param.GetPanelType(panelFace) == PanelType.MaskedCorner)
                    return true;
                var _snapped = panelFace.GetSnappedFace(param.GetPanelEdge(panelFace));
                return (_snapped == sideFace);
            }
            return false;
        }
    }
}
