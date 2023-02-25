using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DialogNPCTarget : GroupMasterAdjunct
    {
        public DialogNPCTarget(DialogGroup dialog)
            : base(dialog, dialog)
        {
        }

        public override object Clone()
            => new DialogNPCTarget(DialogGroup);

        public DialogGroup DialogGroup => Group as DialogGroup;
    }
}
