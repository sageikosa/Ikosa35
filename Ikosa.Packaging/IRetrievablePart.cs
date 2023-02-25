using System;
using System.Collections.Generic;

namespace Ikosa.Packaging
{
    /// <summary>Provides very basic naming and association</summary>
    public interface IRetrievablePart
    {
        /// <summary>Name of the part</summary>
        string PartName { get; }

        /// <summary>Related children</summary>
        IEnumerable<IRetrievablePart> Parts { get; }

        /// <summary>Type name of the part, typically this.GetType().FullName</summary>
        string PartType { get; }
    }
}
