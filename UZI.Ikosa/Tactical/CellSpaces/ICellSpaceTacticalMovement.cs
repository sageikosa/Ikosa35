using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public interface ICellSpaceTacticalMovement
    {

        /// <summary>bits indicating grip potential of face (8*8 grid)</summary>
        HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace);

        /// <summary>Used to indicate that the space is completely valid (totally clear)</summary>
        bool ValidSpace(uint param, MovementBase movement);

        /// <summary>
        /// Boundaries are used to provide hints and data on how this cell can connect to others, 
        /// and that it will require some squeezing if used as a boundary.
        /// Must still use HedralTransitBlocking to confirm suitability for particular configurations.
        /// </summary>
        IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace);

        Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity);

        IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev);

        /// <summary>True if this cell can be entered during a plummet (including being blown or swept away)</summary>
        bool CanPlummetAcross(uint param, MovementBase movement, AnchorFace plummetFace);

        /// <summary>
        /// Offset faces for deflecting on plummet.  
        /// Based on IEnumerable&lt;MovementOpening&gt; retrieved from OpensTowards
        /// </summary>
        IEnumerable<AnchorFace> PlummetDeflection(uint param, MovementBase movement, AnchorFace gravity);

        /// <summary>True if this cellspace is blocked at the specified interior (all highs set to null), face, edge, or corner</summary>
        bool BlockedAt(uint param, MovementBase movement, CellSnap snap);

        Vector3D InteractionOffset3D(uint param);

        /// <summary>Critical points (corners, edge centers, face centers) that can be used for tactical aiming purposes</summary>
        IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement);

        /// <summary>Ensures eight corners provided, each with a list of faces to which it applies</summary>
        IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement);

        /// <summary>Difficulty to grip the surface for climbing</summary>
        int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc);

        /// <summary>Difficulty to grip the surface for climbing</summary>
        CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement);

        /// <summary>Difficulty for swimming</summary>
        int? InnerSwimDifficulty(uint param);
    }

    public static class ICellSpaceTacticalMovementHelper
    {
        /// <summary>Used to see if cell has an edge that can be gripped</summary>
        public static int? OuterCornerGripDifficulty(this ICellSpaceTacticalMovement self, uint param, AnchorFaceList edgeFaces, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            if (self.BlockedAt(param, movement, new CellSnap(edgeFaces)))
                return (from _f in edgeFaces.ToAnchorFaces()
                        let _diff = self.OuterGripDifficulty(param, _f, gravity, movement, sourceStruc)
                        select _diff).Min();
            return null;
        }
    }
}
