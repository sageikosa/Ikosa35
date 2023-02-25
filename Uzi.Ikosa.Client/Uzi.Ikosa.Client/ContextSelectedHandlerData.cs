using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client
{
    public class ContextSelectedHandlerData : EventArgs
    {
        public ContextSelectedHandlerData(IEnumerable<AwarenessInfo> awarenesses)
            : base()
        {
            Awarenesses = awarenesses;
        }

        public IEnumerable<AwarenessInfo> Awarenesses { get; set; }
    }
}
