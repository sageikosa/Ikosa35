using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public interface IModifier : IDelta, IControlTerminate, IControlChange<DeltaValue>
    {
    }
}
