using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    /// <summary>Used to note alterations to interactions</summary>
    public abstract class InteractionAlteration : ISourcedObject
    {
        protected InteractionAlteration(InteractData interactData, object source)
        {
            Source = source;
            InteractData = interactData;
        }

        public object Source { get; private set; }
        public InteractData InteractData { get; private set; }
        public abstract IEnumerable<Info> Information { get; }
    }
}
