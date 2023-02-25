using System;
using System.Collections.Generic;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SlopeComposite : BaseComposite, ISlopeComposite
    {
        public SlopeComposite(string name, SolidCellMaterial material, TileSet tiling, double thick1, double thick2) :
            base(name, material, tiling, thick1)
        {
            _SlopeThickness = thick2;
        }

        private double _SlopeThickness;

        /// <summary>Lesser of the thicknesses</summary>
        public double LesserThickness => Math.Min(Thickness, _SlopeThickness);

        /// <summary>Greater of the thicknesses</summary>
        public double GreaterThickness => Math.Max(Thickness, _SlopeThickness);

        public override double AverageThickness => (Thickness + SlopeThickness) / 2d;

        /// <summary>Maximum thickness is 5</summary>
        protected override bool CanSetThickness(double value)
            => (value >= 0) && (value < 5);

        #region public double SlopeThickness { get; set; }
        public double SlopeThickness
        {
            get => _SlopeThickness;
            set
            {
                if (CanSetThickness(value))
                {
                    _SlopeThickness = value;
                    DoPropertyChanged(nameof(SlopeThickness));
                }
            }
        }
        #endregion

        #region public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        {
            if (movement.CanMoveThrough(Material))
                return false;

            if (param.IsTrueSlope)
            {
                // must be a corner or edge on the slope source face
                return faces.Contains(param.SourceFace);
            }

            return false;
        }
        #endregion

        #region public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        {
            if (movement.CanMoveThrough(Material))
                return new HedralGrip(false);

            if (param.IsTrueSlope)
            {
                if (param.IsFaceSlopeSide(panelFace))
                {
                    // partial coverage
                    return new HedralGrip(panelFace.GetAxis(), param.SourceFace,
                        param.SinkFace.IsLowFace() ? GreaterThickness : LesserThickness,
                        param.SinkFace.IsLowFace() ? LesserThickness : GreaterThickness);
                }
                else if (param.IsFaceSlopeEnd(panelFace))
                {
                    if (panelFace == param.SinkFace)
                    {
                        // sink end is the high thickness
                        return new HedralGrip(panelFace.GetAxis(), param.SourceFace, GreaterThickness);
                    }
                    else
                    {
                        // opposite end is the low thickness
                        return new HedralGrip(panelFace.GetAxis(), param.SourceFace, LesserThickness);
                    }
                }
                else if (param.IsFaceSlopeBottom(panelFace))
                {
                    // if slope source, then complete blockage
                    return new HedralGrip(true);
                }
            }

            // not really slope controlled
            return new HedralGrip(false);
        }
        #endregion

        #region public PlaneListShell GetPlaneListShell(PanelParams param, int z, int y, int x, bool inverse)
        /// <summary>Uses GreaterThickness and LesserThickness to properly shape the Shell</summary>
        public PlaneListShell GetPlaneListShell(PanelParams param, int z, int y, int x, bool inverse)
        {
            // source
            var _source = param.SourceFace;
            var _ortho = _source.GetAxis();

            // use inverse for material fill that needs to close the interior gaps
            var _isUpper = !_source.IsLowFace();
            if (inverse)
                _isUpper = !_isUpper;

            // sink
            var _sink = param.SinkFace;
            var _slope = _sink.GetAxis();

            // offsets
            var _loOffset = _sink.IsLowFace()
                ? (_isUpper ? 5 - GreaterThickness : GreaterThickness)
                : (_isUpper ? 5 - LesserThickness : LesserThickness);
            var _hiOffset = _sink.IsLowFace()
                ? (_isUpper ? 5 - LesserThickness : LesserThickness)
                : (_isUpper ? 5 - GreaterThickness : GreaterThickness);

            // shell
            var _shell = new PlaneListShell();
            foreach (var _plane in SlopeSpaceFaces.GeneratePlanes(z, y, x, _isUpper, _ortho, _slope, _loOffset, _hiOffset))
                _shell.Add(_plane);
            return _shell;
        }
        #endregion

        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
            => GetPlaneListShell(param, z, y, x, false)
            .TransitSegment(p1, p2);

        protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => GetPlaneListShell(param, z, y, x, false)
            .TransitSegment(p1, p2);

        protected override bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
            => IntersectsPanel(param, z, y, x, p1, p2, new BasePanel[] { });

        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => GetPlaneListShell(param, z, y, x, false)
            .Intersects(p1, p2);

        public override void AddInnerStructures(PanelParams param, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, Dictionary<AnchorFace, INaturalPanel> naturalSides, IEnumerable<IBasePanel> interiors)
        {
            SlopeSpaceFaces.AddInnerSlopeComposite(addTogroup, param, Thickness, SlopeThickness, effect, this);
        }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterSlopeComposite(group, param, z, y, x, visibleFace, Thickness, SlopeThickness, effect, this);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible && param.IsTrueSlope)
            {
                if (param.IsFaceSlopeBottom(panelFace))
                {
                    // if slope source, then all occluded
                    return true;
                }

                // otherwise only the sourceface is occluded
                return sideFace == param.SourceFace;
            }
            return false;
        }

        public override bool IsGas => false;
        public override bool IsLiquid => false;
        public override bool IsInvisible => false;
    }
}
