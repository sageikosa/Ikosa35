using System;
using System.Collections.Generic;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class NormalPanel : BaseNaturalPanel
    {
        public NormalPanel(string name, SolidCellMaterial material, TileSet tiling, double thickness) :
            base(name, material, tiling, thickness)
        {
        }

        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
            => movement.CanMoveThrough(Material)
            ? false
            : faces.Contains(panelFace);

        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
            => new HedralGrip(!movement.CanMoveThrough(Material));

        #region private OrthoRectangularPrism GetOrthoRectangularPrism(AnchorFace panelFace, int z, int y, int x)
        private OrthoRectangularPrism GetOrthoRectangularPrism(AnchorFace panelFace, int z, int y, int x)
        {
            // position
            var _pt = new Point3D(x * 5d, y * 5d, z * 5d);

            // only offset for panel face when it is the high face
            Func<Axis, double> _offset =
                (axis) => (panelFace.GetAxis() == axis) && (!panelFace.IsLowFace()) ? 5d - Thickness : 0d;
            _pt.Offset(_offset(Axis.X), _offset(Axis.Y), _offset(Axis.Z));

            // panel size adjusted for panel thickness
            Func<Axis, double> _sizePart =
                (axis) => panelFace.GetAxis() == axis ? Thickness : 5d;
            var _size = new Vector3D(_sizePart(Axis.X), _sizePart(Axis.Y), _sizePart(Axis.Z));

            // prism and transit vector
            var _prism = new OrthoRectangularPrism(_pt, _size);
            return _prism;
        }
        #endregion

        #region protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _prism = GetOrthoRectangularPrism(panelFace, z, y, x);
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
            var _prism = GetOrthoRectangularPrism(panelFace, z, y, x);
            return _prism.Intersects(p1, p2);
        }

        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            return false;
        }

        public override void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            // TODO: determine visiblity?
            PanelSpaceFaces.AddInnerNormalPanel(addTogroup, panelFace, Thickness, effect, this);
        }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            // TODO: determine visiblity?
            if (panelFace == visibleFace)
                PanelSpaceFaces.AddOuterNormalPanel(group, new CellPosition(z, y, x), panelFace, effect, this);
            else
                PanelSpaceFaces.AddOuterNormalPanel(group, z, y, x, panelFace, visibleFace, Thickness, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            // blocks exposure as long as the panel is not invisible
            return !IsInvisible;
        }
    }
}