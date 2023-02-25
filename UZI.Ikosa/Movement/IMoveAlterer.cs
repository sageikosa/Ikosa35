using System.Collections.Generic;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    public interface IMoveAlterer : ICore
    {
        /// <summary>True if movement from SourceCell to TargetCell is legal for the movement</summary>
        bool BlocksTransit(MovementTacticalInfo moveTactical);

        /// <summary>Only uses Movement and SourceCell</summary>
        IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj);

        /// <summary>True if movement is hindered in region</summary>
        bool HindersTransit(MovementBase movement, IGeometricRegion region);

        /// <summary>Used for support against gravity and cell elevations</summary>
        double HedralTransitBlocking(MovementTacticalInfo moveTactical);

        bool IsCostlyMovement(MovementBase movement, IGeometricRegion region);

        /// <summary>True if the affecter allows the region when using the given movement</summary>
        bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj);
    }
}
