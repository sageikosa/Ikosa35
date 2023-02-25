using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class NonPlayerService : ModuleNode
    {
        // TODO: base for shops, services, lodging

        // TODO: not meant to proscribe offerings, but to collect and make easier to handle in-game

        protected NonPlayerService(Description description)
            :base(description)
        {
        }

        public override string GroupName => @"Services";
    }
}
