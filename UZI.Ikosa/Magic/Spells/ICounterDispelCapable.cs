using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    public interface ICounterDispelCapable : ICapability
    {
        IEnumerable<Type> CounterableSpells { get; }
        IEnumerable<Type> DescriptorTypes { get; }
    }
}
