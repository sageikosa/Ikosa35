using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client
{
    public class ContextClickedHandlerData : ContextSelectedHandlerData
    {
        public ContextClickedHandlerData(IEnumerable<AwarenessInfo> awarenesses)
            : base(awarenesses)
        {
        }
    }
}
