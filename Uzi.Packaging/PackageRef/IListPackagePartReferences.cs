using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Packaging
{
    public interface IListPackagePartReferences
    {
        IEnumerable<IPackagePartReference> AllReferences { get; }
    }
}
