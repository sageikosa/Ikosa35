using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class QueuedIntersection : QueuedCell
    {
        public QueuedIntersection(ObservableActor actor, ICellLocation location)
            : base(actor, location)
        {
        }
    }
}
