using System;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IAura
    {
        Guid ID { get; }
        AuraStrength AuraStrength { get; }
    }
}
