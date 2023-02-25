using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions.Action
{
    [Serializable]
    public class Retrieve : InteractData
    {
        public Retrieve(CoreActor actor, IObjectContainer container)
            : base(actor)
        {
            _Repository = container;
        }

        private IObjectContainer _Repository;

        public IObjectContainer Repository => _Repository;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();

        private static RetrieveHandler _Static = new RetrieveHandler();
    }
}
