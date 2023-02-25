using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IActorController
    {
        IActorControlGroup ActorControlGroup { get; }
    }
}
