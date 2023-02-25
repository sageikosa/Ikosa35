using System;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    public interface IModuleElement
    {
        Description Description { get; }
        Guid ID { get; }
    }
}