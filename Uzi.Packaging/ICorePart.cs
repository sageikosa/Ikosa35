using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Packaging
{
    /// <summary>Provides very basic naming and association</summary>
    public interface ICorePart
    {
        /// <summary>Name of the part</summary>
        string Name { get; }

        /// <summary>Related children</summary>
        IEnumerable<ICorePart> Relationships { get; }

        /// <summary>Type name of the part, typically this.GetType().FullName</summary>
        string TypeName { get; }
    }
}
