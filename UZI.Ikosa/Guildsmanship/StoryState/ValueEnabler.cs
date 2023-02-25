using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class ValueEnabler : ModuleElement
    {
        protected ValueEnabler(Description description) 
            : base(description)
        {
        }

        public abstract bool Enablesvalue(TeamTracker tracker, IEnumerable<Creature> creatures);
        public string Name => Description.Message;
    }
}
