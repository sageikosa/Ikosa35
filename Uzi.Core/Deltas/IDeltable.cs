using System.Collections.Generic;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public interface IDeltable : IVolatileValue
    {
        double BaseDoubleValue { get; set; }
        int BaseValue { get; set; }
        Collection<string> DeltaDescriptions { get; }

        /// <summary>Set of IDeltas</summary>
        DeltaSet Deltas { get; }

        DeltableInfo ToDeltableInfo(Qualifier qualifier = null);
        IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify, object baseSource, string baseName);
    }
}