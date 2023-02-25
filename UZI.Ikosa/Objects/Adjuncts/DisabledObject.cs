using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class DisabledObject : Adjunct
    {
        public DisabledObject()
            : base(typeof(DisabledObject))
        {
        }

        public override object Clone()
            => new DisabledObject();

        // TODO: re-enable object if aware that it is disabled...???
    }
}
