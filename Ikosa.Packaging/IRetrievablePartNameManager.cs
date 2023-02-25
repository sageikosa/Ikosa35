using System;
using System.Collections.Generic;

namespace Ikosa.Packaging
{
    /// <summary>Provides name collision avoidance for IRetrievableParts</summary>
    public interface IRetrievablePartNameManager : IRetrievablePart
    {
        /// <summary>Used to avoid name indexing collisions</summary>
        bool CanUseName(string name);

        /// <summary>Used when a part is being renamed, so that any name index should be updated.</summary>
        void Rename(string oldName, string newName);
    }
}
