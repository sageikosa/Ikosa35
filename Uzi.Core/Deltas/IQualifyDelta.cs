using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>
    /// Simple qualify delta supplier.  
    /// Used in SoftDelta since it doesn't need to know about termination.
    /// </summary>
    public interface ISupplyQualifyDelta
    {
        IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify);
    }

    /// <summary>Deltas that apply only under qualified conditions</summary>
    public interface IQualifyDelta : ISupplyQualifyDelta, IControlTerminate
    {
    }
}
