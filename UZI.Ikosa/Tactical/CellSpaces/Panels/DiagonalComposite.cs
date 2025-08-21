using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class DiagonalComposite : BaseComposite
    {
        public DiagonalComposite(string name, SolidCellMaterial material, TileSet tiling) :
            base(name, material, tiling, 0)
        {
        }

        protected override bool CanSetThickness(double value)
            => false;

        #region public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        public override bool BlockedAt(PanelParams param, AnchorFace panelFace, MovementBase movement, List<AnchorFace> faces)
        {
            if (movement.CanMoveThrough(Material))
            {
                return false;
            }

            // must have some connection to the panel face
            if (!faces.Contains(panelFace))
            {
                return false;
            }

            if ((param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace)))
            {
                // anything on the face is blocked
                return true;
            }
            else if (param.IsFaceDiagonalSide(panelFace))
            {
                // panel is triangular side, so only blocks along shared edges
                var _controlFaces = param.DiagonalFaceControlFaces(panelFace);
                return (_controlFaces.Count() == 2) && _controlFaces.Intersects(faces);
            }
            else if (param.IsFaceTriangularSink(panelFace))
            {
                // panel is triangular sink, so only blocks along shared edges
                var _controlFaces = param.TriangularSinkEdges(panelFace);
                return (_controlFaces != AnchorFaceList.None) && _controlFaces.Intersects(faces);
            }
            return false;
        }
        #endregion

        public override HedralGrip HedralGripping(PanelParams param, AnchorFace panelFace, MovementBase movement, IEnumerable<BasePanel> transitPanels)
        {
            if (movement.CanMoveThrough(Material))
            {
                return new HedralGrip(false);
            }
            else if (param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace))
            {
                // just like a normal panel
                return new HedralGrip(true);
            }
            else if (param.IsFaceDiagonalSide(panelFace) || param.IsFaceTriangularSink(panelFace))
            {
                // diagonal fill is in force, blocks half
                return new HedralGrip();
            }

            return new HedralGrip(false);
        }

        #region public static PlaneListShell GetPlaneListShell(PanelParams param, int z, int y, int x, bool inverse)
        /// <summary>Very generic diagonals, offset by a vector dependent on z,y,x</summary>
        public static PlaneListShell GetPlaneListShell(PanelParams param, int z, int y, int x, bool inverse)
        {
            var _shell = new PlaneListShell();
            var _offset = new System.Windows.Media.Media3D.Vector3D(x * 5d, y * 5d, z * 5d);
            if (param.DiagonalControls != AnchorFaceList.None)
            {
                if (param.OtherFace != OptionalAnchorFace.None)
                {
                    if (inverse)
                    {
                        _shell.Add(PlaneListShellBuilder.GetPyramid(_offset, param.SinkFace.ReverseFace(), param.SourceFace.ReverseFace(), param.OtherFace.ToAnchorFace().ReverseFace()));
                    }
                    else
                    {
                        _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SinkFace, param.SourceFace));
                        _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SinkFace, param.OtherFace.ToAnchorFace()));
                    }
                }
                else
                {
                    if (inverse)
                    {
                        _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SourceFace.ReverseFace(), param.SinkFace.ReverseFace()));
                    }
                    else
                    {
                        _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SourceFace, param.SinkFace));
                    }
                }
            }
            else if (param.BendControls != AnchorFaceList.None)
            {
                if (inverse)
                {
                    _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SourceFace.ReverseFace(), param.SinkFace.ReverseFace()));
                    _shell.Add(PlaneListShellBuilder.GetTriangularPrism(_offset, param.SourceFace.ReverseFace(), param.OtherFace.ToAnchorFace().ReverseFace()));
                }
                else
                {
                    _shell.Add(PlaneListShellBuilder.GetPyramid(_offset, param.SourceFace, param.SinkFace, param.OtherFace.ToAnchorFace()));
                }
            }

            return _shell;
        }
        #endregion

        protected override bool IntersectsPanel(PanelParams param, AnchorFace panelFace, int z, int y, int x, Point3D p1, Point3D p2)
            => false;

        protected override bool IntersectsPanel(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
            => GetPlaneListShell(param, z, y, x, false).Intersects(p1, p2);

        #region protected override double TransitLength(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        protected override double TransitLength(PanelParams param, int z, int y, int x, Point3D p1, Point3D p2, IEnumerable<BasePanel> interiors)
        {
            var _segment = TransitSegment(param, z, y, x, p1, p2, interiors);
            if (_segment != null)
            {
                // double diagonal?
                if ((param.DiagonalControls != AnchorFaceList.None) && (param.OtherFace != OptionalAnchorFace.None))
                {
                    // see if segment goes through material fill
                    var _mFill = DiagonalComposite.GetPlaneListShell(param, z, y, x, true);
                    var _mSegment = _mFill.TransitSegment(p1, p2);
                    if (_mSegment.HasValue && _mSegment.Value.WithinBoundsOf(_segment.Value))
                    {
                        // take out material fill section
                        if (_mSegment.Value.Vector.LengthSquared < _segment.Value.Vector.LengthSquared)
                        {
                            return _segment.Value.Vector.Length - _mSegment.Value.Vector.Length;
                        }
                    }
                }
                else
                {
                    // no negative segment
                    return _segment.Value.Vector.Length;
                }
            }

            // no segment
            return 0d;
        }
        #endregion

        protected override Segment3D? TransitSegment(PanelParams param, AnchorFace panelFace, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2)
            => null;

        protected override Segment3D? TransitSegment(PanelParams param, int z, int y, int x, System.Windows.Media.Media3D.Point3D p1, System.Windows.Media.Media3D.Point3D p2, IEnumerable<BasePanel> interiors)
            => DiagonalComposite.GetPlaneListShell(param, z, y, x, false).TransitSegment(p1, p2);

        public override void AddInnerStructures(PanelParams param, BuildableGroup addTogroup, int z, int y, int x, VisualEffect effect, Dictionary<AnchorFace, INaturalPanel> naturalSides, IEnumerable<IBasePanel> interiors)
        {
            PanelSpaceFaces.AddInnerDiagonalComposite(addTogroup, param, effect, this);
        }

        public override void AddOuterSurface(PanelParams param, BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace visibleFace, VisualEffect effect, Vector3D bump, IEnumerable<IBasePanel> transitPanels)
        {
            PanelSpaceFaces.AddOuterDiagonalComposite(group, param, z, y, x, panelFace, effect, this, bump);
        }

        public override bool OrthoOcclusion(PanelParams param, AnchorFace panelFace, AnchorFace sideFace)
        {
            if (!IsInvisible)
            {
                if (param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace))
                {
                    // full side acts like a normal panel
                    return true;
                }
                else if (param.IsFaceDiagonalSide(panelFace))
                {
                    return param.DiagonalFaceControlFaces(panelFace).Contains(sideFace);
                }
                else if (param.IsFaceTriangularSink(panelFace))
                {
                    return param.TriangularSinkEdges(panelFace).Contains(sideFace);
                }
            }
            return false;
        }

        public override bool IsGas => false;
        public override bool IsLiquid => false;
        public override bool IsInvisible => false;
    }
}
