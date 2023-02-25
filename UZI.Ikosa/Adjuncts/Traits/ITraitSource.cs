using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Adjuncts
{
    public interface ITraitSource
    {
    }

    public interface ITraitPowerClassSource : ITraitSource
    {
        IPowerClass PowerClass { get; }
    }
}
