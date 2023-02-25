using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IActorControlGroup
    {
        IActorController ActorController { get; }
        IActorControlled ActorControlled { get; }
    }
}
