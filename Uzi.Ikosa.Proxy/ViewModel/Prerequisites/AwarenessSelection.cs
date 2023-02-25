using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AwarenessSelection
    {
        public AwarenessInfo Awareness { get; set; }
        public RelayCommand<AwarenessSelection> SelectCmd { get; set; }
        public virtual Guid ID => Awareness.ID;
    }

    public class NoneAwarenessSelection : AwarenessSelection
    {
        public override Guid ID => Guid.Empty;
    }
}
