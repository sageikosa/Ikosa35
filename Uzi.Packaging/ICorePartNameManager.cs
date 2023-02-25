using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Packaging
{
    /// <summary>Provides name collision avoidance for ICoreParts</summary>
    public interface ICorePartNameManager : ICorePart
    {
        /// <summary>Used to avoid name indexing collisions</summary>
        bool CanUseName(string name, Type partType);

        /// <summary>Used when a part is being renamed, so that any name index should be updated.</summary>
        void Rename(string oldName, string newName, Type partType);
    }
}
