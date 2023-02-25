using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public abstract class NeedsTurnTickInitiativeOrder : ViewModelBase
    {
        public abstract Guid ID { get; }
    }

    public class NeedsTurnTickExistingTickOrder : NeedsTurnTickInitiativeOrder
    {
        public TickInfo TickInfo { get; set; }
        public override Guid ID => TickInfo?.FirstBudget?.ActorID ?? Guid.Empty;
    }

    public class NeedsTurnTickAddCreatureOrder : NeedsTurnTickInitiativeOrder
    {
        public CreatureTrackerModel CreatureTrackerModel { get; set; }
        public override Guid ID => CreatureTrackerModel?.ID ?? Guid.Empty;
    }
}
