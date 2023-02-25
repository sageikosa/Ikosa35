using System;
using System.Collections.Generic;
using System.Text;

namespace Ikosa.Packaging
{
    public interface IListPartReferences
    {
        IEnumerable<IStorablePart> AllReferences { get; }
    }
}
