using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LFramePanel : BaseNaturalPanel, ILFramePanel
    {
        public LFramePanel(string name, SolidCellMaterial material, TileSet tiling, double thickness, double horizWidth, double vertWidth)
            : base(name, material, tiling, thickness)
        {
            _Horiz = horizWidth;
            _Vert = vertWidth;
        }

        #region data
        private double _Horiz;
        private double _Vert;
        #endregion

        #region public double HorizontalWidth { get; set; }
        public double HorizontalWidth
        {
            get => _Horiz;
            set
            {
                _Horiz = value;
                DoPropertyChanged(nameof(HorizontalWidth));
            }
        }
        #endregion

        #region public double VerticalWidth { get; set; }
        public double VerticalWidth
        {
            get => _Vert;
            set
            {
                _Vert = value;
                DoPropertyChanged(nameof(VerticalWidth));
            }
        }
        #endregion

        #region public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        {
            if (movement.CanMoveThrough(Material))
            {
                return false;
            }

            // framing edge faces
            var _snap = param.GetPanelCorner(panelFace);
            var _edgeFaces = _snap.GetEdgeFaces(panelFace);

            // only blocked if _faces represents either snapped edge or corners
            return faces.Contains(panelFace) && _edgeFaces.Intersects(faces);
        }
        #endregion

        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        {
            if (movement.CanMoveThrough(Material))
            {
                return new HedralGrip(false);
            }

            // area
            var _axis = panelFace.GetAxis();
            var _corner = param.GetPanelCorner(panelFace);

            return new HedralGrip(_axis, panelFace.GetSnappedFace(_corner.HorizontalEdge()), HorizontalWidth)
                .Union(new HedralGrip(_axis, panelFace.GetSnappedFace(_corner.VerticalEdge()), VerticalWidth));
        }

        #region protected override double TransitLength(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        protected override double TransitLength(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _segment = TransitSegment(param, panelFace, z, y, x, p1, p2);
            if (_segment.HasValue && (_segment.Value.Vector.LengthSquared > 0))
            {
                var _panelAxis = panelFace.GetAxis();
                var _edges = param.GetPanelCorner(panelFace).GetEdgeFaces(panelFace);
                var _axis1 = Axis.X;
                var _axis2 = Axis.Y;
                switch (panelFace.GetAxis())
                {
                    case Axis.Y:
                        _axis1 = Axis.X;
                        _axis2 = Axis.Z;
                        break;
                    case Axis.X:
                        _axis1 = Axis.Y;
                        _axis2 = Axis.Z;
                        break;
                }

                // positions
                var _pt = new Point3D(x * 5d, y * 5d, z * 5d);

                // offset for panel face axis, and snap axis when each is not it's axial low face
                Func<Axis, double> _offset = (axis) =>
                {
                    // panel thickness
                    if (_panelAxis == axis)
                    {
                        return (!panelFace.IsLowFace() ? 5d - Thickness : 0d);
                    }
                    else if (_axis1 == axis)
                    {
                        // axis1 is low, so offset past Width1
                        if (_edges.ToAnchorFaces().Any(_e => (_e.GetAxis() == _axis1) && _e.IsLowFace()))
                        {
                            return HorizontalWidth;
                        }
                    }
                    else if (_axis2 == axis)
                    {
                        // axis2 is low, so offset past Width2
                        if (_edges.ToAnchorFaces().Any(_e => (_e.GetAxis() == _axis2) && _e.IsLowFace()))
                        {
                            return VerticalWidth;
                        }
                    }
                    return 0d;
                };
                _pt.Offset(_offset(Axis.X), _offset(Axis.Y), _offset(Axis.Z));

                // thickness, snap, or full extent
                Func<Axis, double> _sizePart = (axis) =>
                {
                    if (_panelAxis == axis)
                    {
                        return Thickness;
                    }
                    else if (_axis1 == axis)
                    {
                        return 5 - VerticalWidth;
                    }
                    else if (_axis2 == axis)
                    {
                        return 5 - HorizontalWidth;
                    }

                    return 5;
                };
                var _size = new Vector3D(_sizePart(Axis.X), _sizePart(Axis.Y), _sizePart(Axis.Z));
                var _notch = new OrthoRectangularPrism(_pt, _size);
                var _nVect = _notch.TransitSegment(p1, p2);
                if (_nVect.HasValue && _nVect.Value.WithinBoundsOf(_segment.Value))
                {
                    // transit some of the notch
                    if (_nVect.Value.Vector.LengthSquared < _segment.Value.Vector.LengthSquared)
                    {
                        return _segment.Value.Vector.Length - _nVect.Value.Vector.Length;
                    }
                }
                else
                {
                    // didn't transit the notch, so all through the panel
                    return _segment.Value.Vector.Length;
                }
            }

            // no transit of the panel
            return 0d;
        }
        #endregion

        #region private PlaneListShell GetPlaneListShell(ref PanelParams param, AnchorFace panelFace, int z, int y, int x)
        private PlaneListShell GetPlaneListShell(ref PanelParams param, AnchorFace panelFace, int z, int y, int x)
        {
            var _panelAxis = panelFace.GetAxis();
            var _edges = param.GetPanelCorner(panelFace).GetEdgeFaces(panelFace);
            var _axis1 = Axis.X;
            var _axis2 = Axis.Y;
            switch (panelFace.GetAxis())
            {
                case Axis.Y:
                    _axis1 = Axis.X;
                    _axis2 = Axis.Z;
                    break;
                case Axis.X:
                    _axis1 = Axis.Y;
                    _axis2 = Axis.Z;
                    break;
            }

            // positions
            var _pt1 = new Point3D(x * 5d, y * 5d, z * 5d);
            var _pt2 = new Point3D(x * 5d, y * 5d, z * 5d);

            // offset for panel face axis, and snap axis when each is not it's axial low face
            Func<Axis, Axis, double, double> _offset = (axis, frameAxis, width) =>
            {
                // panel thickness
                if (_panelAxis == axis)
                {
                    return (!panelFace.IsLowFace() ? 5d - Thickness : 0d);
                }
                else if (frameAxis == axis)
                {
                    // axis is not low, so offset to 5-width
                    if (_edges.ToAnchorFaces().Any(_e => (_e.GetAxis() == frameAxis) && !_e.IsLowFace()))
                    {
                        return 5d - width;
                    }
                }
                return 0d;
            };
            _pt1.Offset(_offset(Axis.X, _axis1, VerticalWidth), _offset(Axis.Y, _axis1, VerticalWidth), _offset(Axis.Z, _axis1, VerticalWidth));
            _pt2.Offset(_offset(Axis.X, _axis2, HorizontalWidth), _offset(Axis.Y, _axis2, HorizontalWidth), _offset(Axis.Z, _axis2, HorizontalWidth));

            // thickness, snap, or full extent
            Func<Axis, Axis, double, double> _sizePart = (axis, frameAxis, width) =>
            {
                if (_panelAxis == axis)
                {
                    return Thickness;
                }
                else if (frameAxis == axis)
                {
                    return width;
                }

                return 5;
            };
            var _size1 = new Vector3D(_sizePart(Axis.X, _axis1, VerticalWidth), _sizePart(Axis.Y, _axis1, VerticalWidth), _sizePart(Axis.Z, _axis1, VerticalWidth));
            var _size2 = new Vector3D(_sizePart(Axis.X, _axis1, HorizontalWidth), _sizePart(Axis.Y, _axis2, HorizontalWidth), _sizePart(Axis.Z, _axis2, HorizontalWidth));

            // geom and transit vector
            var _shell = new PlaneListShell();
            _shell.Add(new OrthoRectangularPrism(_pt1, _size1));
            _shell.Add(new OrthoRectangularPrism(_pt2, _size2));
            return _shell;
        }
        #endregion

        #region protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _shell = GetPlaneListShell(ref param, panelFace, z, y, x);
            return _shell.TransitSegment(p1, p2);
        }
        #endregion

        protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => null;

        protected override bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
        {
            var _shell = GetPlaneListShell(ref param, panelFace, z, y, x);
            return _shell.Intersects(p1, p2);
        }

        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => false;

        public override void AddInnerStructures(PanelParams param, AnchorFace panelFace, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, IEnumerable<IBasePanel> interiors)
        {
            // TODO: determine visiblity?
            PanelSpaceFaces.AddInnerLFramePanel(addTogroup, panelFace, Thickness, param.GetPanelCorner(panelFace), HorizontalWidth, VerticalWidth, effect, this);
        }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            // TODO: determine visiblity?
            if (param.GetPanelType(panelFace) == PanelType.MaskedCorner)
            {
                if (panelFace == visibleFace)
                {
                    PanelSpaceFaces.AddOuterNormalPanel(group, new CellPosition(z, y, x), panelFace, effect, this);
                }
                else
                {
                    PanelSpaceFaces.AddOuterNormalPanel(group, z, y, x, panelFace, visibleFace, Thickness, effect, this);
                }
            }
            else
            {
                PanelSpaceFaces.AddOuterLFramePanel(group, z, y, x, panelFace, visibleFace, Thickness, param.GetPanelCorner(panelFace),
                    HorizontalWidth, VerticalWidth, effect, this);
            }
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                if (param.GetPanelType(panelFace) == PanelType.MaskedLFrame)
                {
                    return true;
                }

                var _edges = param.GetPanelCorner(panelFace).GetEdgeFaces(panelFace);
                return _edges.Contains(sideFace);
            }
            return false;
        }
    }
}