using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Creatures.Templates
{
    public interface ICreatureTemplate
    {
        bool IsAcquired { get; }
        string TemplateName { get; }
    }
}
