using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Interface presenting model keys by object state key in a uniform manner.
    /// Consumed by object, managed by editor (usually), or magical effects...
    /// </summary>
    public interface IObjectStateModels
    {
        IEnumerable<ObjectStateModelKey> StateModelKeys { get; }
    }
}
