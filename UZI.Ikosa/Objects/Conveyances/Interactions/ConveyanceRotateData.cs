using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceRotateData : InteractData
    {
        public ConveyanceRotateData(CoreActor actor, string direction)
            : base(actor)
        {
            _Direction = direction;
        }

        private string _Direction;

        public string Direction => _Direction;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return ConveyanceRotateHandler.Static;
            yield break;
        }
    }
}
