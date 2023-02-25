using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class MaterialFill : BasePanel
    {
        public MaterialFill(string name, CellMaterial material, TileSet tiling)
            : base(name, material, tiling, 0)
        {
        }

        protected override bool CanSetThickness(double value)
            => false;

        #region public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        {
            // must be able to block
            if (movement.CanMoveThrough(Material))
                return false;

            // must have some connection to the panel face
            if (!faces.Contains(panelFace))
                return false;

            if (param.IsFaceDiagonalSide(panelFace))
            {
                // panel is triangular side, so only blocks along shared edges
                var _controlFaces = param.DiagonalFaceControlFaces(panelFace);
                return (_controlFaces.Count() == 2) && _controlFaces.Intersects(faces.Select(_f => _f.ReverseFace()));
            }
            else if (param.IsFaceTriangularSink(panelFace))
            {
                // panel is triangular sink, so only blocks along shared edges
                var _controlFaces = param.TriangularSinkEdges(panelFace);
                return (_controlFaces != AnchorFaceList.None) && _controlFaces.Intersects(faces.Select(_f => _f.ReverseFace()));
            }
            else if (param.IsFaceSlopeSide(panelFace) || param.IsFaceSlopeEnd(panelFace))
            {
                // only apply blocking opposite the slope source face
                return faces.Contains(param.SourceFace.ReverseFace());
            }
            else
            {
                switch (param.GetPanelType(panelFace))
                {
                    case PanelType.Corner:
                    case PanelType.MaskedCorner:
                        {
                            // fill for a corner
                            var _snap = param.GetPanelEdge(panelFace);
                            return faces.Contains(panelFace.GetSnappedFace(_snap).ReverseFace());
                        }

                    case PanelType.LFrame:
                    case PanelType.MaskedLFrame:
                        {
                            // fill for an L-Frame
                            var _snap = param.GetPanelCorner(panelFace);
                            var _edgeFaces = _snap.GetEdgeFaces(panelFace);
                            return _edgeFaces.IsSubset(faces.Select(_f => _f.ReverseFace()));
                        }
                }
            }
            return true;
        }
        #endregion

        #region public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        {
            if (movement.CanMoveThrough(Material))
                // no blocking, since material doesn't block
                return new HedralGrip(false);

            if (param.IsFaceDiagonalSide(panelFace) || param.IsFaceTriangularSink(panelFace))
            {
                // panel is triangular
                return new HedralGrip((param.GetDiagonalGrip(panelFace) ?? TriangleCorner.LowerLeft).GetFullSwap());
            }
            else if (param.IsFaceSlopeSide(panelFace))
            {
                // slope side
                var _slope = transitPanels.OfType<SlopeComposite>().FirstOrDefault();
                if (_slope != null)
                    return new HedralGrip(panelFace.GetAxis(), param.SourceFace.ReverseFace(),
                        5 - (param.SinkFace.IsLowFace() ? _slope.GreaterThickness : _slope.LesserThickness),
                        5 - (param.SinkFace.IsLowFace() ? _slope.LesserThickness : _slope.GreaterThickness));
            }
            else if (param.IsFaceSlopeEnd(panelFace))
            {
                // slope end
                var _slope = transitPanels.OfType<SlopeComposite>().FirstOrDefault();
                if (_slope != null)
                    return new HedralGrip(panelFace.GetAxis(), param.SourceFace,
                        ((param.SinkFace == panelFace) ? _slope.GreaterThickness : _slope.LesserThickness));
            }
            else
            {
                switch (param.GetPanelType(panelFace))
                {
                    case PanelType.Corner:
                    case PanelType.MaskedCorner:
                        var _corner = transitPanels.OfType<CornerPanel>().FirstOrDefault();
                        if (_corner != null)
                        {
                            // fractional area not covered by the corner
                            return new HedralGrip(
                                panelFace.GetAxis(),
                                panelFace.GetSnappedFace(param.GetPanelEdge(panelFace).GetFullSwap()),
                                5 - _corner.Offset);
                        }
                        break;

                    case PanelType.LFrame:
                    case PanelType.MaskedLFrame:
                        var _lFrame = transitPanels.OfType<LFramePanel>().FirstOrDefault();
                        if (_lFrame != null)
                        {
                            // fractional area not covered by the LFrame
                            var _axis = panelFace.GetAxis();
                            var _tri = param.GetPanelCorner(panelFace);

                            return new HedralGrip(_axis, panelFace.GetSnappedFace(_tri.HorizontalEdge().GetFullSwap()), 5 - _lFrame.HorizontalWidth)
                                .Union(new HedralGrip(_axis, panelFace.GetSnappedFace(_tri.VerticalEdge().GetFullSwap()), 5 - _lFrame.VerticalWidth));
                        }
                        break;

                    case PanelType.NoPanel:
                    default:
                        return new HedralGrip(true);
                }
            }

            // no blocking, since nothing got picked up
            return new HedralGrip(false);
        }
        #endregion

        protected override bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2)
            => false;

        #region protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2, IEnumerable<BasePanel> interiors)
        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2, IEnumerable<BasePanel> interiors)
        {
            if ((param.DiagonalControls != AnchorFaceList.None) || (param.BendControls != AnchorFaceList.None))
            {
                return DiagonalComposite.GetPlaneListShell(param, z, y, x, true).Intersects(p1, p2);
            }
            else if (param.IsTrueSlope)
            {
                // inverse of slope panel
                var _slope = interiors.OfType<SlopeComposite>().FirstOrDefault();
                if (_slope != null)
                    return _slope.GetPlaneListShell(param, z, y, x, true).Intersects(p1, p2);
            }

            // uniform fill
            return true;
        }
        #endregion

        #region protected override double TransitLength(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        protected override double TransitLength(PanelParams param, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2, IEnumerable<BasePanel> interiors)
        {
            var _segment = TransitSegment(param, z, y, x, p1, p2, interiors);
            if (_segment.HasValue && (_segment.Value.Vector.LengthSquared > 0))
            {
                // bend
                if (param.BendControls != AnchorFaceList.None)
                {
                    // see if segment goes through bend
                    var _bFill = DiagonalComposite.GetPlaneListShell(param, z, y, x, false);
                    var _bSegment = _bFill.TransitSegment(p1, p2);
                    if (_bSegment.HasValue && _bSegment.Value.WithinBoundsOf(_segment.Value))
                    {
                        // take out bend section
                        if (_bSegment.Value.Vector.LengthSquared < _segment.Value.Vector.LengthSquared)
                            return _segment.Value.Vector.Length - _bSegment.Value.Vector.Length;
                    }
                }
                else
                    // no negative segment
                    return _segment.Value.Vector.Length;
            }

            // no transit?
            return 0d;
        }
        #endregion

        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2)
            => null;

        #region protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2, IEnumerable<BasePanel> interiors)
        {
            if ((param.DiagonalControls != AnchorFaceList.None) || (param.BendControls != AnchorFaceList.None))
            {
                return DiagonalComposite.GetPlaneListShell(param, z, y, x, true).TransitSegment(p1, p2);
            }
            else if (param.IsTrueSlope)
            {
                // inverse of slope panel
                var _slope = interiors.OfType<SlopeComposite>().FirstOrDefault();
                if (_slope != null)
                    return _slope.GetPlaneListShell(param, z, y, x, true).TransitSegment(p1, p2);
            }

            // entire segment when uniform fill
            return new Segment3D(p1, p2);
        }
        #endregion

        #region public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Transform3D bump, IEnumerable<BasePanel> transitPanels)
        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump,
            IEnumerable<IBasePanel> transitPanels)
        {
            if (IsInvisible)
                return;

            PanelSpaceFaces.AddOuterMaterialFill(group, z, y, x, panelFace, this, effect, bump);
        }
        #endregion

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            // fill never blocks real sides
            return false;
        }
    }
}
