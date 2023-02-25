using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Provides an implementation of ISimpleStep for SimpleStep to use</summary>
    public interface ISimpleStep
    {
        bool DoStep(CoreStep actualStep);
    }
}
