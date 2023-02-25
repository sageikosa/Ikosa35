using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public abstract class ActorControlled<CtrlGroup> : GroupMemberAdjunct, IActorControlled
        where CtrlGroup : ActorControlGroup
    {
        protected ActorControlled(object source, CtrlGroup controlGroup)
            : base(source, controlGroup)
        {
        }

        public CtrlGroup ControlGroup => Group as CtrlGroup;
        public IActorControlGroup ActorControlGroup => ControlGroup;
    }
}
