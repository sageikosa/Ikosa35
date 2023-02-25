using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ObjectManipulateData : InteractData
    {
        public ObjectManipulateData(CoreActor actor, string direction)
            : base(actor)
        {
            _Direction = direction;
        }

        private string _Direction;

        public string Direction => _Direction;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            if (target is Furnishing)
            {
                yield return FurnishingRotateHandler.Static;
            }
            yield break;
        }
    }
}
