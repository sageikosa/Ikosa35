using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    public abstract class ActorControlGroup : AdjunctGroup, IActorControlGroup
    {
        protected ActorControlGroup(object source) 
            : base(source)
        {
        }

        public IActorController ActorController => Members.OfType<IActorController>().FirstOrDefault();
        public IActorControlled ActorControlled => Members.OfType<IActorControlled>().FirstOrDefault();
    }
}
