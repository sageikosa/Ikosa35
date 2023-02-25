using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class RootDestruction : InteractData
    {
        public RootDestruction(ICoreObject root)
            : base(null)
        {
            _Root = root;
        }

        private ICoreObject _Root;

        public ICoreObject Root => _Root;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            if ((target is Mechanism) || (target is Drawer))
            {
                yield return new EjectOnRootDestructionHandler();
            }
            else
            {
                yield return new DestroyOnRootDestructionHandler();
            }
            yield break;
        }

    }
}
